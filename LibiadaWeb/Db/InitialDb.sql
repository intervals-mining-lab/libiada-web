--17.08.2014 23:45:20
BEGIN;

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';

CREATE EXTENSION IF NOT EXISTS plv8 WITH SCHEMA pg_catalog;

COMMENT ON EXTENSION plv8 IS 'PL/JavaScript (v8) trusted procedural language';

CREATE EXTENSION IF NOT EXISTS pg_trgm;

CREATE FUNCTION check_element_in_alphabet(chain_id bigint, element_id bigint) RETURNS boolean
    LANGUAGE plv8
    AS $_$var plan = plv8.prepare( 'SELECT count(*) = 1 result FROM (SELECT unnest(alphabet) a FROM chain WHERE id = $1) c WHERE c.a = $2', ['bigint', 'bigint']);
var result = plan.execute([chain_id, element_id])[0].result;
plan.free();
return result; $_$;

COMMENT ON FUNCTION check_element_in_alphabet(chain_id bigint, element_id bigint) IS 'Функция для проверки наличия элемента с указанным id в алфавите указанной цепочки.';

CREATE FUNCTION check_genes_import_positions(matter_id bigint) RETURNS void
    LANGUAGE plv8 STABLE
    AS $_$function check_genes_import_positions(id){
	plv8.elog(INFO, "Проверяем Позиции всех фрагментов генома объекта с id =", id);
	
	var genes = plv8.execute('SELECT piece_position "start", piece_position + array_length(building, 1) "stop" FROM dna_chain WHERE matter_id = $1 AND piece_type_id != 1 ORDER BY piece_position;', [id]);
	
	if(genes[0].start != 0){
		plv8.elog(WARNING, "Начало первого фрагмента не на нулевой позиции. Фактическая позиция начала первого фрагмента: ", genes[0].start);
	}
	
	for(var i = 1; i < genes.length; i++){
		if(genes[i].start > genes[i - 1].stop){
			plv8.elog(WARNING, "Начало и конец соседних фрагментов не совпадают. Конец предыдущего фрагмента: ", genes[i - 1].stop, " Начало следующего: ", genes[i].start);
		}
	}
	
	var fullChainLength = plv8.execute('SELECT array_length(building, 1) length FROM dna_chain WHERE matter_id = $1 AND piece_type_id = 1;', [id])[0].length;
	
	if(genes[genes.length - 1].stop != fullChainLength){
		plv8.elog(WARNING, "Конец последнего фрагмента не совпадает с длиной полной последовательности. Фактическая позиция конца последнего фрагмента: ", genes[genes.length - 1].stop, " Длина полной цепи: ", fullChainLength);
	}
}

check_genes_import_positions(matter_id);$_$;

COMMENT ON FUNCTION check_genes_import_positions(matter_id bigint) IS 'Функция, проверяющая, что все фрагменты полной генетической последовательности загружены в БД. ';

CREATE FUNCTION copy_constraints(src text, dst text) RETURNS integer
    LANGUAGE plpgsql IMMUTABLE
    AS $$
begin
  return copy_constraints(src::regclass::oid, dst::regclass::oid);
end;
$$;

CREATE FUNCTION copy_constraints(srcoid oid, dstoid oid) RETURNS integer
    LANGUAGE plpgsql
    AS $$
declare
  i int4 := 0;
  constrs record;
  srctable text;
  dsttable text;
begin
  srctable = srcoid::regclass;
  dsttable = dstoid::regclass;
  for constrs in
  select conname as name, pg_get_constraintdef(oid) as definition
  from pg_constraint where conrelid = srcoid loop
    begin
    execute 'alter table ' || dsttable
      || ' add constraint '
      || replace(replace(constrs.name, srctable, dsttable),'.','_')
      || ' ' || constrs.definition;
    i = i + 1;
    exception
      when duplicate_table then
    end;
  end loop;
  return i;
exception when undefined_table then
  return null;
end;
$$;

CREATE FUNCTION copy_indexes(src text, dst text) RETURNS integer
    LANGUAGE plpgsql IMMUTABLE
    AS $$
begin
  return copy_indexes(src::regclass::oid, dst::regclass::oid);
end;
$$;

CREATE FUNCTION copy_indexes(srcoid oid, dstoid oid) RETURNS integer
    LANGUAGE plpgsql
    AS $$
declare
  i int4 := 0;
  indexes record;
  srctable text;
  dsttable text;
  script text;
begin
  srctable = srcoid::regclass;
  dsttable = dstoid::regclass;
  for indexes in
  select c.relname as name, pg_get_indexdef(idx.indexrelid) as definition
  from pg_index idx, pg_class c where idx.indrelid = srcoid and c.oid = idx.indexrelid loop
    script = replace (indexes.definition, ' INDEX '
      || indexes.name, ' INDEX '
      || replace(replace(indexes.name, srctable, dsttable),'.','_'));
    script = replace (script, ' ON ' || srctable, ' ON ' || dsttable);
    begin
      execute script;
      i = i + 1;
    exception
      when duplicate_table then
    end;
  end loop;
  return i;
exception when undefined_table then
  return null;
end;
$$;

CREATE FUNCTION copy_triggers(src text, dst text) RETURNS integer
    LANGUAGE plpgsql IMMUTABLE
    AS $$
begin
  return copy_triggers(src::regclass::oid, dst::regclass::oid);
end;
$$;

CREATE FUNCTION copy_triggers(srcoid oid, dstoid oid) RETURNS integer
    LANGUAGE plpgsql
    AS $$
declare
  i int4 := 0;
  triggers record;
  srctable text;
  dsttable text;
  script text = '';
begin
  srctable = srcoid::regclass;
  dsttable = dstoid::regclass;
  for triggers in
   select tgname as name, pg_get_triggerdef(oid) as definition
   from pg_trigger where tgrelid = srcoid loop
    script =
    replace (triggers.definition, ' TRIGGER '
      || triggers.name, ' TRIGGER '
      || replace(replace(triggers.name, srctable, dsttable),'.','_'));
    script = replace (script, ' ON ' || srctable, ' ON ' || dsttable);
    begin
      execute script;
      i = i + 1;
    exception
      when duplicate_table then
    end;
  end loop;
  return i;
exception when undefined_table then
  return null;
end;
$$;

CREATE FUNCTION db_integrity_test() RETURNS void
    LANGUAGE plv8
    AS $$
function CheckChain() {
    plv8.elog(INFO, "Проверяем целостность таблицы chain (уникальность id, наличие алфавита и строя у всех цепочек)");

    var chain = plv8.execute('SELECT id FROM chain');
    var chainDistinct = plv8.execute('SELECT DISTINCT id FROM chain');
    if (chain.length != chainDistinct.length) {
        plv8.elog(ERROR, 'id таблицы chain и/или дочерних таблиц не уникальны.');
    }else{
	plv8.elog(INFO, "id всех цепочек уникальны.");
    }

    plv8.elog(INFO, "Проверяем соответствие всех записей таблицы chain и её наследников с записями в таблице chain_key");
    
    var chainDisproportion = plv8.execute('SELECT c.id, ck.id FROM chain c FULL OUTER JOIN chain_key ck ON ck.id = c.id WHERE c.id IS NULL OR ck.id IS NULL');
    
    if (chainDisproportion.length > 0) {
        plv8.elog(ERROR, 'Количество записей в таблице chain_key не совпадает с количеством записей с таблице chain и её наследниках. Для подробностей выполните "SELECT c.id, ck.id FROM chain c FULL OUTER JOIN chain_key ck ON ck.id = c.id WHERE c.id IS NULL OR ck.id IS NULL"');
    }else{
	plv8.elog(INFO, "Все записи в таблицах цепочек однозначно соответствуют записям в таблице chain_key.");
    }
}

function CheckElement() {
    plv8.elog(INFO, "Проверяем целостность таблицы element и её потмков");

    var element = plv8.execute('SELECT id FROM element');
    var elementDistinct = plv8.execute('SELECT DISTINCT id FROM element');
    if (element.length != elementDistinct.length) {
        plv8.elog(ERROR, 'id таблицы element и/или дочерних таблиц не уникальны.');
    }else{
	plv8.elog(INFO, "id всех элементов уникальны.");
    }

    plv8.elog(INFO, "Проверяем соответствие всех записей таблицы element и её наследников с записями в таблице element_key");
    
    var elementDisproportion = plv8.execute('SELECT c.id, ck.id FROM element c FULL OUTER JOIN element_key ck ON ck.id = c.id WHERE c.id IS NULL OR ck.id IS NULL');
    
    if (elementDisproportion.length > 0) {
        plv8.elog(ERROR, 'Количество записей в таблице element_key не совпадает с количеством записей с таблице element и её наследниках. Для подробностей выполните "SELECT c.id, ck.id FROM element c FULL OUTER JOIN element_key ck ON ck.id = c.id WHERE c.id IS NULL OR ck.id IS NULL"');
    }else{
	plv8.elog(INFO, "Все записи в таблицах элементов однозначно соответствуют записям в таблице element_key.");
    }
}

function CheckAlphabet() {
   //TODO: написать проверки того что все элементы алфавита есть в таблице element или её наследниках. 
   //TODO: Проверить что все бинарные и однородныее характеристики вычислены для элементов присутствующих в алфавите.
}

function db_integrity_test() {
    plv8.elog(INFO, "Процедура проверки целостности БД запущена.");
    CheckChain();
    CheckElement();
    CheckAlphabet();
    plv8.elog(INFO, "Проверка целостности успешно завершена.");
}

db_integrity_test();$$;

COMMENT ON FUNCTION db_integrity_test() IS 'Функция для проверки целостности данных в базе.';

CREATE FUNCTION trigger_building_check() RETURNS trigger
    LANGUAGE plv8
    AS $$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "INSERT"){
	var building = NEW.building;
	var max = 0;
	building.forEach(function(element, index){
		if(element > max + 1){
			plv8.elog(ERROR, 'Проверка строя не пройдена, нарушение в ', index, 'элементе.');
		}
		else if(element == max + 1){
			max = element;
		}
	});
	if(max == NEW.alphabet.length){
		return NEW;
	} else{
		plv8.elog(ERROR, 'Максимальное значение в строе не совпадает с длинной алфавита.');
	}
	
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операции добавления записей в таблице с полем building.');
}$$;

COMMENT ON FUNCTION trigger_building_check() IS 'Триггерная функция, проверяющая что строй соответствует ограничениям накладываемым на строй.';

CREATE FUNCTION trigger_chain_key_bound() RETURNS trigger
    LANGUAGE plv8
    AS $_$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "INSERT"){
	plv8.execute( 'INSERT INTO chain_key VALUES ($1)', [NEW.id] );
	return NEW;
}
else if (TG_OP == "UPDATE" && NEW.id != OLD.id){
	plv8.execute( 'UPDATE chain_key SET id = $1 WHERE id = $2', [NEW.id, OLD.id] );
	return NEW;
} if (TG_OP == "DELETE"){
	plv8.execute( 'DELETE FROM chain_key WHERE id = $1', [OLD.id] );
	return OLD;
}$_$;

COMMENT ON FUNCTION trigger_chain_key_bound() IS 'Триггерная функция связывающая действия с указанной таблицей (добавление, обновление, удаление) с таблицей chain_key.';

CREATE FUNCTION trigger_chain_key_insert() RETURNS trigger
    LANGUAGE plv8
    AS $_$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "INSERT"){
 var result = plv8.execute('SELECT count(*) = 1 result FROM chain WHERE id = $1', [NEW.id])[0].result;
 if (result){
  return NEW;
 }else{
  var gene = plv8.execute('SELECT count(*) = 1 result FROM gene WHERE id = $1', [NEW.id])[0].result;
  if (gene){
   return NEW;
  }else{
   plv8.elog(ERROR, 'Нельзя добавлять запись в ', TG_TABLE_NAME, ' без предварительного добавления записи в таблицу chain или её потомок. id = ', NEW.id);
  }
 }
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операции добавления записей в таблице с полем id.');
}
$_$;

CREATE FUNCTION trigger_characteristics_link() RETURNS trigger
    LANGUAGE plv8
    AS $_$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "INSERT" || TG_OP == "UPDATE"){
	var linkOk = plv8.execute('SELECT linkable=($1 IS NOT NULL) AS result FROM characteristic_type WHERE id = $2;', [NEW.link_id, NEW.characteristic_type_id])[0].result;
	if(linkOk){
		return NEW;
	}
	else{
		plv8.elog(ERROR, 'Добавлемая характеристика имеет неверную привязку.');
	}
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей в таблице с полями characteristic_type_id и link_id');
}
$_$;

COMMENT ON FUNCTION trigger_characteristics_link() IS 'Триггерная функция, проверяющая что у характеристики не задана привязка если она непривязываема, и задана любая привязка если она необходима.';

CREATE FUNCTION trigger_check_alphabet() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
	orphanedElements integer;
BEGIN
	IF (TG_OP = 'INSERT' OR TG_OP = 'UPDATE') THEN
		SELECT count(1) INTO orphanedElements result FROM (SELECT DISTINCT unnest(alphabet) a FROM chain) c LEFT OUTER JOIN element_key e ON e.id = c.a WHERE e.id IS NULL;
		IF (orphanedElements = 0) THEN 
			RETURN NULL;
		ELSE
			RAISE EXCEPTION 'В БД отсутствует % элементов алфавита.', orphanedElements;
		END IF;
	END IF;
    RAISE EXCEPTION 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей в таблице с полем alphabet.';
END;$$;

COMMENT ON FUNCTION trigger_check_alphabet() IS 'Триггерная функция, проверяющая что все элементы алфавита добавляемой цепочки есть в базе.';

CREATE FUNCTION trigger_check_applicability() RETURNS trigger
    LANGUAGE plv8
    AS $_$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "INSERT" || TG_OP == "UPDATE"){
	var applicabilityOk = plv8.execute('SELECT ' + TG_ARGV[0] + ' AS result FROM characteristic_type WHERE id = $1;', [NEW.characteristic_type_id])[0].result;
	if(applicabilityOk){
		return NEW;
	}
	else{
		plv8.elog(ERROR, 'Добавлемая характеристика неприменима к данному типу цепочки.');
	}
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей в таблице с полями characteristic_type_id.');
}
$_$;

COMMENT ON FUNCTION trigger_check_applicability() IS 'Триггерная функция, проверяющая, что характеристика может быть вычислена для такого типа цепочки';

CREATE FUNCTION trigger_check_element_in_alphabet() RETURNS trigger
    LANGUAGE plv8
    AS $$//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "INSERT" || TG_OP == "UPDATE"){
	var check_element_in_alphabet = plv8.find_function("check_element_in_alphabet");
	var elementInAlphabet = check_element_in_alphabet(NEW.chain_id, NEW.element_id);
	if(elementInAlphabet){
		return NEW;
	}
	else{
		plv8.elog(ERROR, 'Добавлемая характеристика привязана к элементу, отсутствующему в алфавите цепочки. element_id = ', NEW.element_id,' ; chain_id = ', NEW.chain_id);
	}
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей в таблице с полями chain_id и element_id');
}$$;

COMMENT ON FUNCTION trigger_check_element_in_alphabet() IS 'Триггерная функция, проверяющая что элемент для которого вычислена характеристика однородной цепи есть в алфавите указанной цепочки. По сути замена для внешнего ключа ссылающегося на алфавит.';

CREATE FUNCTION trigger_check_elements_in_alphabet() RETURNS trigger
    LANGUAGE plv8
    AS $$//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "INSERT" || TG_OP == "UPDATE"){
	var check_element_in_alphabet = plv8.find_function("check_element_in_alphabet");
	var firstElementInAlphabet = check_element_in_alphabet(NEW.chain_id, NEW.first_element_id);
	var secondElementInAlphabet = check_element_in_alphabet(NEW.chain_id, NEW.second_element_id);
	if(firstElementInAlphabet && secondElementInAlphabet){
		return NEW;
	}
	else if(firstElementInAlphabet){
		plv8.elog(ERROR, 'Добавлемая характеристика привязана к элементу, отсутствующему в алфавите цепочки. second_element_id = ', NEW.second_element_id,' ; chain_id = ', NEW.chain_id);
	} else{
		plv8.elog(ERROR, 'Добавлемая характеристика привязана к элементу, отсутствующему в алфавите цепочки. first_element_id = ', NEW.first_element_id,' ; chain_id = ', NEW.chain_id);
	}
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей в таблице с полями chain_id, first_element_id и second_element_id');
}$$;

COMMENT ON FUNCTION trigger_check_elements_in_alphabet() IS 'Триггерная функция, проверяющая что элементы для которых вычислен коэффициент зависимости присутствуют в алфавите указанной цепочки. По сути замена для внешних ключей ссылающихся на алфавит.';

CREATE FUNCTION trigger_delete_chain_characteristics() RETURNS trigger
    LANGUAGE plv8
    AS $$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "INSERT" || TG_OP == "UPDATE"){
	plv8.execute('DELETE FROM characteristic USING chain c WHERE characteristic.chain_id = c.id AND characteristic.created < c.modified;');
	plv8.execute('DELETE FROM binary_characteristic USING chain c WHERE binary_characteristic.chain_id = c.id AND binary_characteristic.created < c.modified;');
	plv8.execute('DELETE FROM congeneric_characteristic USING chain c WHERE congeneric_characteristic.chain_id = c.id AND congeneric_characteristic.created < c.modified;');
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей.');
}

$$;

COMMENT ON FUNCTION trigger_delete_chain_characteristics() IS 'Триггерная функция, удаляющая все характеристики при удалении или изменении цепочки.';

CREATE FUNCTION trigger_element_delete_alphabet_bound() RETURNS trigger
    LANGUAGE plv8
    AS $_$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);
if (TG_OP == "DELETE"){
	var elementUsedCount = plv8.execute('SELECT count(*) result FROM (SELECT DISTINCT unnest(alphabet) a FROM chain) c WHERE c.a = $1', [OLD.id])[0].result;
	if (elementUsedCount == 0){
		return OLD;
	} else{
		plv8.elog(ERROR, 'Невозможно удалить элемент, он всё ещё используется в алфавите одной или нескольких цепочек. id = ', OLD.id);
	}
	
} else{
	plv8.elog(ERROR, 'Неизвестная операция. данный триггер нработает только с опредацией удаления из таблиц, имеющих поле id. TG_OP = ', TG_OP);
}$_$;

CREATE FUNCTION trigger_element_key_bound() RETURNS trigger
    LANGUAGE plv8
    AS $_$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "INSERT"){
	plv8.execute( 'INSERT INTO element_key VALUES ($1)', [NEW.id] );
	return NEW;
}
else if (TG_OP == "UPDATE" && NEW.id != OLD.id){
	plv8.execute( 'UPDATE element_key SET id = $1 WHERE id = $2', [NEW.id, OLD.id] );
	return NEW;
} else if (TG_OP == "DELETE"){
	plv8.execute( 'DELETE FROM element_key WHERE id = $1', [OLD.id] );
	return OLD;
}$_$;

COMMENT ON FUNCTION trigger_element_key_bound() IS 'Триггерная функция связывающая действия с указанной таблицей (добавление, обновление) с таблицей element_key.';

CREATE FUNCTION trigger_element_key_insert() RETURNS trigger
    LANGUAGE plv8
    AS $_$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);
if (TG_OP == "INSERT"){
 var result = plv8.execute('SELECT count(*) = 1 result FROM element WHERE id = $1', [NEW.id])[0].result;
 if (result){
  return NEW;
 }else{
  plv8.elog(ERROR, 'Нельзя добавлять запись в element_key без предварительного добавления записи в таблицу element или её потомок. id = ', NEW.id);
 }
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операции добавления записей в таблице с полем id.');
}$_$;

CREATE FUNCTION trigger_element_update_alphabet() RETURNS trigger
    LANGUAGE plv8
    AS $_$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "UPDATE"){
	var oldVal = OLD.id;
	var newVal = NEW.id;
	var chains = plv8.execute('UPDATE chain SET alphabet = c1.alphabet FROM (SELECT c1.id, array_replace(c1.alphabet, $1, $2) alphabet FROM chain c1 WHERE alphabet @> ARRAY[$1]) c1 WHERE chain.id = c1.id;', [OLD.id, NEW.id]);
	return NEW;
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей в таблице с полем id.');
}$_$;

COMMENT ON FUNCTION trigger_element_update_alphabet() IS 'Автоматически обновляет алфавит при обновлении элементов.';

CREATE FUNCTION trigger_set_modified() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
    BEGIN
	IF (TG_OP = 'INSERT') THEN
            NEW.created := now();
        END IF;
        IF (TG_OP = 'INSERT' OR TG_OP = 'UPDATE') THEN
            NEW.modified := now();
            RETURN NEW;
        END IF;
        RAISE EXCEPTION 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей в таблицах с полями modified и created.';
    END;
$$;

COMMENT ON FUNCTION trigger_set_modified() IS 'Триггерная функция, добавляющая текущее время и дату в поля modified и created.';

SET default_tablespace = '';

SET default_with_oids = false;

CREATE TABLE accidental (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    description text
);

COMMENT ON TABLE accidental IS 'Справочная таблица знаков альтерации.';

COMMENT ON COLUMN accidental.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN accidental.name IS 'Название знака альтерации.';

COMMENT ON COLUMN accidental.description IS 'Описание знака альтерации.';

CREATE SEQUENCE accidental_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE accidental_id_seq OWNED BY accidental.id;

CREATE TABLE binary_characteristic (
    id bigint NOT NULL,
    chain_id bigint NOT NULL,
    characteristic_type_id integer NOT NULL,
    value double precision,
    value_string text,
    link_id integer,
    created timestamp with time zone DEFAULT now() NOT NULL,
    first_element_id bigint NOT NULL,
    second_element_id bigint NOT NULL,
    modified timestamp with time zone DEFAULT now() NOT NULL
);

COMMENT ON TABLE binary_characteristic IS 'Таблица со значениями характеристик зависимостей элементов.';

COMMENT ON COLUMN binary_characteristic.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN binary_characteristic.chain_id IS 'Цепочка для которой вычислялась характеристика.';

COMMENT ON COLUMN binary_characteristic.characteristic_type_id IS 'Вычисляемая характеристика.';

COMMENT ON COLUMN binary_characteristic.value IS 'Числовое значение характеристики.';

COMMENT ON COLUMN binary_characteristic.value_string IS 'Строковое значение характеристики.';

COMMENT ON COLUMN binary_characteristic.link_id IS 'Привязка (если она используется).';

COMMENT ON COLUMN binary_characteristic.created IS 'Дата вычисления характеристики.';

COMMENT ON COLUMN binary_characteristic.first_element_id IS 'id первого элемента из пары для которой вычисляется характеристика.';

COMMENT ON COLUMN binary_characteristic.second_element_id IS 'id второго элемента из пары для которой вычисляется характеристика.';

COMMENT ON COLUMN binary_characteristic.modified IS 'Дата и время последнего изменения записи в таблице.';

CREATE SEQUENCE binary_characteristic_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE binary_characteristic_id_seq OWNED BY binary_characteristic.id;

CREATE TABLE element (
    id bigint NOT NULL,
    value character varying(255),
    description text,
    name character varying(255),
    notation_id integer NOT NULL,
    created timestamp with time zone DEFAULT now() NOT NULL,
    modified timestamp with time zone DEFAULT now() NOT NULL
);

COMMENT ON TABLE element IS 'Элементы цепочек.';

COMMENT ON COLUMN element.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN element.value IS 'Содержимое элемента, записываемое в цепочку.';

COMMENT ON COLUMN element.description IS 'Описание элемента.';

COMMENT ON COLUMN element.name IS 'Название элемента.';

COMMENT ON COLUMN element.notation_id IS 'Форма записи элемента.';

COMMENT ON COLUMN element.created IS 'Дата создания записи.';

COMMENT ON COLUMN element.modified IS 'Дата и время последнего изменения записи в таблице.';

CREATE SEQUENCE elements_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE elements_id_seq OWNED BY element.id;

CREATE TABLE chain (
    id bigint DEFAULT nextval('elements_id_seq'::regclass) NOT NULL,
    notation_id integer NOT NULL,
    created timestamp with time zone DEFAULT now() NOT NULL,
    matter_id bigint NOT NULL,
    dissimilar boolean DEFAULT false NOT NULL,
    piece_type_id integer DEFAULT 1 NOT NULL,
    piece_position bigint DEFAULT 0 NOT NULL,
    alphabet bigint[] NOT NULL,
    building integer[] NOT NULL,
    remote_id character varying(255),
    remote_db_id integer,
    modified timestamp with time zone DEFAULT now() NOT NULL,
    description text,
    CONSTRAINT chk_remote_id CHECK ((((remote_db_id IS NULL) AND (remote_id IS NULL)) OR ((remote_db_id IS NOT NULL) AND (remote_id IS NOT NULL))))
);

COMMENT ON TABLE chain IS 'Таблица строёв цепочек и других параметров.';

COMMENT ON COLUMN chain.id IS 'Уникальный внутренний идентификатор цепочки.';

COMMENT ON COLUMN chain.notation_id IS 'Форма записи цепочки в зависимости от элементов (буквы, слова, нуклеотиды, триплеты, etc).';

COMMENT ON COLUMN chain.created IS 'Дата создания цепочки.';

COMMENT ON COLUMN chain.matter_id IS 'Ссылка на объект исследования.';

COMMENT ON COLUMN chain.dissimilar IS 'Флаг разнородности.';

COMMENT ON COLUMN chain.piece_type_id IS 'Тип фрагмента.';

COMMENT ON COLUMN chain.piece_position IS 'Позиция фрагмента.';

COMMENT ON COLUMN chain.alphabet IS 'Алфавит цепочки.';

COMMENT ON COLUMN chain.building IS 'Строй цепочки.';

COMMENT ON COLUMN chain.remote_id IS 'id цепочки в удалённой БД.';

COMMENT ON COLUMN chain.remote_db_id IS 'id удалённой базы данных, из которой взята данная цепочка.';

COMMENT ON COLUMN chain.modified IS 'Дата и время последнего изменения записи в таблице.';

COMMENT ON COLUMN chain.description IS 'Описание отдельной цепочки.';

CREATE TABLE chain_key (
    id bigint NOT NULL
);

COMMENT ON TABLE chain_key IS 'Суррогатная таблица, хрянящая ключи всех таблиц-цепочек.';

COMMENT ON COLUMN chain_key.id IS 'Уникальный идентификатор цепочки.';

CREATE TABLE characteristic (
    id bigint NOT NULL,
    chain_id bigint NOT NULL,
    characteristic_type_id integer NOT NULL,
    value double precision,
    value_string text,
    link_id integer,
    created timestamp with time zone DEFAULT now() NOT NULL,
    modified timestamp with time zone DEFAULT now() NOT NULL
);

COMMENT ON TABLE characteristic IS 'Таблица со значениями различных характеристик цепочек.';

COMMENT ON COLUMN characteristic.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN characteristic.chain_id IS 'Цепочка для которой вычислялась характеристика.';

COMMENT ON COLUMN characteristic.characteristic_type_id IS 'Вычисляемая характеристика.';

COMMENT ON COLUMN characteristic.value IS 'Числовое значение характеристики.';

COMMENT ON COLUMN characteristic.value_string IS 'Строковое значение характеристики.';

COMMENT ON COLUMN characteristic.link_id IS 'Привязка (если она используется).';

COMMENT ON COLUMN characteristic.created IS 'Дата вычисления характеристики.';

COMMENT ON COLUMN characteristic.modified IS 'Дата и время последнего изменения записи в таблице.';

CREATE TABLE characteristic_group (
    id integer NOT NULL,
    name character varying(100),
    description text
);

COMMENT ON TABLE characteristic_group IS 'Справочник принадлежности характеристик той или иной группе.';

COMMENT ON COLUMN characteristic_group.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN characteristic_group.name IS 'Тип характеристики.';

COMMENT ON COLUMN characteristic_group.description IS 'Описание типа.';

CREATE SEQUENCE characteristic_group_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE characteristic_group_id_seq OWNED BY characteristic_group.id;

CREATE TABLE characteristic_type (
    id integer NOT NULL,
    name character varying(100),
    description text,
    characteristic_group_id integer,
    class_name character varying(255) NOT NULL,
    linkable boolean DEFAULT true NOT NULL,
    full_chain_applicable boolean DEFAULT false NOT NULL,
    congeneric_chain_applicable boolean DEFAULT false NOT NULL,
    binary_chain_applicable boolean DEFAULT false NOT NULL,
    CONSTRAINT chk_characteristic_applicable CHECK (((full_chain_applicable OR binary_chain_applicable) OR congeneric_chain_applicable))
);

COMMENT ON TABLE characteristic_type IS 'Таблица видов характеристик';

COMMENT ON COLUMN characteristic_type.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN characteristic_type.name IS 'Название харакетристики.';

COMMENT ON COLUMN characteristic_type.description IS 'Описание харакетристики.';

COMMENT ON COLUMN characteristic_type.characteristic_group_id IS 'Группа которой принадлежит характеристика.';

COMMENT ON COLUMN characteristic_type.class_name IS 'Название класса калькулятора.';

COMMENT ON COLUMN characteristic_type.linkable IS 'Флаг, определяющий, ипользуется ли привязка при вычислении данной характерристики.';

COMMENT ON COLUMN characteristic_type.full_chain_applicable IS 'Флаг ,указывающий, что данная характеристика применима к полным цепочкам.';

COMMENT ON COLUMN characteristic_type.congeneric_chain_applicable IS 'Флаг ,указывающий, что данная характеристика применима к однородным цепочкам.';

COMMENT ON COLUMN characteristic_type.binary_chain_applicable IS 'Флаг ,указывающий, что данная характеристика применима к бинарным цепочкам.';

COMMENT ON CONSTRAINT chk_characteristic_applicable ON characteristic_type IS 'Проверяет что характеристика применима хотя бы к одному типу цепочек.';

CREATE SEQUENCE characteristic_type_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE characteristic_type_id_seq OWNED BY characteristic_type.id;

CREATE SEQUENCE characteristics_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE characteristics_id_seq OWNED BY characteristic.id;

CREATE TABLE congeneric_characteristic (
    id bigint NOT NULL,
    chain_id bigint NOT NULL,
    characteristic_type_id integer NOT NULL,
    value double precision,
    value_string text,
    link_id integer,
    created timestamp with time zone DEFAULT now() NOT NULL,
    element_id bigint NOT NULL,
    modified timestamp with time zone DEFAULT now() NOT NULL
);

COMMENT ON TABLE congeneric_characteristic IS 'Таблица хранящая характеристики однородных цепочек.
Не используется прямое наследование чтобы избежать выборки одноимённых однородных характеристик вместе с характеристиками полных цепей.';

COMMENT ON COLUMN congeneric_characteristic.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN congeneric_characteristic.chain_id IS 'Цепочка для которой вычислялась характеристика.';

COMMENT ON COLUMN congeneric_characteristic.characteristic_type_id IS 'Вычисляемая характеристика.';

COMMENT ON COLUMN congeneric_characteristic.value IS 'Числовое значение характеристики.';

COMMENT ON COLUMN congeneric_characteristic.value_string IS 'Строковое значение характеристики.';

COMMENT ON COLUMN congeneric_characteristic.link_id IS 'Привязка (если она используется).';

COMMENT ON COLUMN congeneric_characteristic.created IS 'Дата вычисления характеристики.';

COMMENT ON COLUMN congeneric_characteristic.element_id IS 'Ссылка на элемент однородной цепочки, для которого вычислена характеристика.';

COMMENT ON COLUMN congeneric_characteristic.modified IS 'Дата и время последнего изменения записи в таблице.';

CREATE SEQUENCE congeneric_characteristic_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE congeneric_characteristic_id_seq OWNED BY congeneric_characteristic.id;

CREATE TABLE dna_chain (
    fasta_header character varying(255),
    web_api_id integer,
    complement boolean DEFAULT false NOT NULL,
    partial boolean DEFAULT false NOT NULL,
    product_id integer
)
INHERITS (chain);

COMMENT ON TABLE dna_chain IS 'Таблица содержащая цепочки ДНК, наследованная от таблицы chain.';

COMMENT ON COLUMN dna_chain.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN dna_chain.notation_id IS 'Форма записи.';

COMMENT ON COLUMN dna_chain.created IS 'Дата создания.';

COMMENT ON COLUMN dna_chain.matter_id IS 'Объект исследования.';

COMMENT ON COLUMN dna_chain.dissimilar IS 'Флаг разнородности.';

COMMENT ON COLUMN dna_chain.piece_type_id IS 'Тип фрагмента.';

COMMENT ON COLUMN dna_chain.piece_position IS 'Позиция фрагмента.';

COMMENT ON COLUMN dna_chain.alphabet IS 'Алфавит цепочки.';

COMMENT ON COLUMN dna_chain.building IS 'Строй цепочки.';

COMMENT ON COLUMN dna_chain.remote_id IS 'id цепочки в удалённой БД.';

COMMENT ON COLUMN dna_chain.remote_db_id IS 'id удалённой базы данных, из которой взята данная цепочка.';

COMMENT ON COLUMN dna_chain.fasta_header IS 'Заголовок fasta файла из которого была извлечена данная цепочка.';

COMMENT ON COLUMN dna_chain.web_api_id IS 'id цепочки в удалённой БД.';

COMMENT ON COLUMN dna_chain.complement IS 'Флаг комплементарности данной последовательности по отношению к полной.';

COMMENT ON COLUMN dna_chain.partial IS 'Флаг указывающий на неполноту последовательности.';

COMMENT ON COLUMN dna_chain.product_id IS 'Тип генетической последовательности.';

CREATE TABLE element_key (
    id bigint NOT NULL
);

COMMENT ON TABLE element_key IS 'Таблица, содаржащая id всех таблиц элементов.';

COMMENT ON COLUMN element_key.id IS 'Уникальный идентификатор.';

CREATE TABLE fmotiv (
    id bigint DEFAULT nextval('elements_id_seq'::regclass),
    notation_id integer DEFAULT 6,
    matter_id bigint DEFAULT 508,
    description text,
    value character varying(255),
    name character varying(255),
    fmotiv_type_id integer NOT NULL
)
INHERITS (chain, element);

COMMENT ON TABLE fmotiv IS 'Таблица ф-мотивов.';

COMMENT ON COLUMN fmotiv.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN fmotiv.notation_id IS 'Форма записи. Рудиментное поле, которое всегда принимает значение 6.';

COMMENT ON COLUMN fmotiv.created IS 'Дата создания.';

COMMENT ON COLUMN fmotiv.matter_id IS 'Объект исследования. Рудиментное поле, которое всегда принимает значение 509.';

COMMENT ON COLUMN fmotiv.dissimilar IS 'Флаг разнородности.';

COMMENT ON COLUMN fmotiv.piece_type_id IS 'Тип фрагмента.';

COMMENT ON COLUMN fmotiv.piece_position IS 'Позиция фрагмента.';

COMMENT ON COLUMN fmotiv.alphabet IS 'Алфавит цепочки.';

COMMENT ON COLUMN fmotiv.building IS 'Строй цепочки.';

COMMENT ON COLUMN fmotiv.remote_id IS 'id цепочки в удалённой БД.';

COMMENT ON COLUMN fmotiv.remote_db_id IS 'id удалённой базы данных, из которой взята данная цепочка.';

COMMENT ON COLUMN fmotiv.description IS 'Описание Ф-мотива.';

COMMENT ON COLUMN fmotiv.value IS 'Ф-мотив в виде строки.';

COMMENT ON COLUMN fmotiv.name IS 'Название Ф-мотива.';

COMMENT ON COLUMN fmotiv.fmotiv_type_id IS 'Тип Ф-мотива.';

CREATE TABLE fmotiv_type (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    description text
);

COMMENT ON TABLE fmotiv_type IS 'Справочная таблица типов ф-мотивов.';

COMMENT ON COLUMN fmotiv_type.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN fmotiv_type.name IS 'Название типа Ф-мотива.';

COMMENT ON COLUMN fmotiv_type.description IS 'Описание типа ф-мотива.';

CREATE SEQUENCE fmotiv_type_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE fmotiv_type_id_seq OWNED BY fmotiv_type.id;

CREATE TABLE gene (
    id bigint DEFAULT nextval('elements_id_seq'::regclass) NOT NULL,
    created timestamp with time zone DEFAULT now() NOT NULL,
    modified timestamp with time zone DEFAULT now() NOT NULL,
    chain_id bigint NOT NULL,
    piece_type_id integer NOT NULL,
    description text NOT NULL,
    web_api_id integer,
    complement boolean DEFAULT false NOT NULL,
    partial boolean DEFAULT false NOT NULL,
    product_id integer
);

COMMENT ON TABLE gene IS 'Таблица с данными о расположении генов и других фрагменов в полном геноме.';

COMMENT ON COLUMN gene.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN gene.created IS 'Дата создания.';

COMMENT ON COLUMN gene.modified IS 'Дата изменения.';

COMMENT ON COLUMN gene.chain_id IS 'Родительская цепочка.';

COMMENT ON COLUMN gene.piece_type_id IS 'Тип фрагмента.';

COMMENT ON COLUMN gene.description IS 'Описание фрагмента.';

COMMENT ON COLUMN gene.web_api_id IS 'id в удалённой БД.';

COMMENT ON COLUMN gene.complement IS 'Флаг комплементарности данной последовательности по отношению к полной.';

COMMENT ON COLUMN gene.partial IS 'Флаг указывающий на неполноту последовательности.';

COMMENT ON COLUMN gene.product_id IS 'Тип генетической последовательности.';

CREATE TABLE instrument (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    description character varying(255)
);

COMMENT ON TABLE instrument IS 'Справочня таблица инструментов.';

COMMENT ON COLUMN instrument.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN instrument.name IS 'Название инструмента.';

COMMENT ON COLUMN instrument.description IS 'Описание инструмента.';

CREATE SEQUENCE instrument_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE instrument_id_seq OWNED BY instrument.id;

CREATE TABLE language (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    description text
);

COMMENT ON TABLE language IS 'Язык литературных текстов.';

COMMENT ON COLUMN language.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN language.name IS 'Язык.';

COMMENT ON COLUMN language.description IS 'Описание.';

CREATE SEQUENCE language_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE language_id_seq OWNED BY language.id;

CREATE TABLE link (
    id integer NOT NULL,
    name character varying(100),
    description text
);

COMMENT ON TABLE link IS 'Таблица видов привязки.';

COMMENT ON COLUMN link.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN link.name IS 'Название привязки.';

COMMENT ON COLUMN link.description IS 'Описание привязки.';

CREATE SEQUENCE link_id_seq
    START WITH 5
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE link_id_seq OWNED BY link.id;

CREATE TABLE literature_chain (
    original boolean DEFAULT true NOT NULL,
    language_id integer NOT NULL,
    translator_id integer
)
INHERITS (chain);

COMMENT ON TABLE literature_chain IS 'Таблица с литературными текстами, наследованная от chain.';

COMMENT ON COLUMN literature_chain.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN literature_chain.notation_id IS 'Форма записи.';

COMMENT ON COLUMN literature_chain.created IS 'Дата создания.';

COMMENT ON COLUMN literature_chain.matter_id IS 'Объект исследования.';

COMMENT ON COLUMN literature_chain.dissimilar IS 'Флаг разнородности.';

COMMENT ON COLUMN literature_chain.piece_type_id IS 'Тип фрагмента.';

COMMENT ON COLUMN literature_chain.piece_position IS 'Позиция фрагмента.';

COMMENT ON COLUMN literature_chain.alphabet IS 'Алфавит цепочки.';

COMMENT ON COLUMN literature_chain.building IS 'Строй цепочки.';

COMMENT ON COLUMN literature_chain.remote_id IS 'id цепочки в удалённой БД.';

COMMENT ON COLUMN literature_chain.remote_db_id IS 'id удалённой базы данных, из которой взята данная цепочка.';

COMMENT ON COLUMN literature_chain.original IS 'Является ли текст оригинальным или же переведённым.';

COMMENT ON COLUMN literature_chain.language_id IS 'Язык.';

COMMENT ON COLUMN literature_chain.translator_id IS 'Ссылка на автора перевода.';

CREATE TABLE matter (
    id bigint NOT NULL,
    name character varying(255) NOT NULL,
    nature_id integer NOT NULL,
    description text,
    created timestamp with time zone DEFAULT now() NOT NULL,
    modified timestamp with time zone DEFAULT now() NOT NULL
);

COMMENT ON TABLE matter IS 'Таблица объектов исследований, сущностей и т.п.';

COMMENT ON COLUMN matter.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN matter.name IS 'Имя объекта.';

COMMENT ON COLUMN matter.nature_id IS 'Природа или происхождение объекта.';

COMMENT ON COLUMN matter.description IS 'Описание объекта.';

COMMENT ON COLUMN matter.created IS 'Дата и время создания объекта исследования.';

COMMENT ON COLUMN matter.modified IS 'Дата и время последнего изменения записи в таблице.';

CREATE SEQUENCE matter_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE matter_id_seq OWNED BY matter.id;

CREATE TABLE measure (
    id bigint DEFAULT nextval('elements_id_seq'::regclass),
    notation_id integer DEFAULT 7,
    matter_id bigint DEFAULT 509,
    description text,
    value character varying(255),
    name character varying(255),
    beats integer NOT NULL,
    beatbase integer NOT NULL,
    ticks_per_beat integer,
    fifths integer NOT NULL,
    major boolean NOT NULL
)
INHERITS (chain, element);

COMMENT ON TABLE measure IS 'Таблица цепочек-элементов - тактов музыкального произведения.';

COMMENT ON COLUMN measure.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN measure.notation_id IS 'Форма записи. Рудиментное поле, которое всегда принимает значение 7.';

COMMENT ON COLUMN measure.created IS 'Дата создания.';

COMMENT ON COLUMN measure.matter_id IS 'Объект исследования. Рудиментное поле, которое всегда принимает значение 509.';

COMMENT ON COLUMN measure.dissimilar IS 'Флаг разнородности.';

COMMENT ON COLUMN measure.piece_type_id IS 'Тип фрагмента.';

COMMENT ON COLUMN measure.piece_position IS 'Позиция фрагмента.';

COMMENT ON COLUMN measure.alphabet IS 'Алфавит цепочки.';

COMMENT ON COLUMN measure.building IS 'Строй цепочки.';

COMMENT ON COLUMN measure.remote_id IS 'id цепочки в удалённой БД.';

COMMENT ON COLUMN measure.remote_db_id IS 'id удалённой базы данных, из которой взята данная цепочка.';

COMMENT ON COLUMN measure.description IS 'Описание.';

COMMENT ON COLUMN measure.value IS 'Такт в виде строки.';

COMMENT ON COLUMN measure.name IS 'Название.';

COMMENT ON COLUMN measure.beats IS 'Числитель размера.';

COMMENT ON COLUMN measure.beatbase IS 'Знаменатель размера.';

COMMENT ON COLUMN measure.ticks_per_beat IS 'Количество тиков за 1 долю размера.';

COMMENT ON COLUMN measure.fifths IS 'Количество знаков при ключе (положительное число - диезы, отрицательное - бемоли).';

COMMENT ON COLUMN measure.major IS 'Лад. true - мажор, false - минор.';

CREATE TABLE music_chain (
)
INHERITS (chain);

COMMENT ON TABLE music_chain IS 'Таблица музыкальных цепочек.';

COMMENT ON COLUMN music_chain.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN music_chain.notation_id IS 'Форма записи.';

COMMENT ON COLUMN music_chain.created IS 'Дата создания.';

COMMENT ON COLUMN music_chain.matter_id IS 'Объект исследования.';

COMMENT ON COLUMN music_chain.dissimilar IS 'Флаг разнородности.';

COMMENT ON COLUMN music_chain.piece_type_id IS 'Тип фрагмента.';

COMMENT ON COLUMN music_chain.piece_position IS 'Позиция фрагмента.';

COMMENT ON COLUMN music_chain.alphabet IS 'Алфавит цепочки.';

COMMENT ON COLUMN music_chain.building IS 'Строй цепочки.';

COMMENT ON COLUMN music_chain.remote_id IS 'id цепочки в удалённой БД.';

COMMENT ON COLUMN music_chain.remote_db_id IS 'id удалённой базы данных, из которой взята данная цепочка.';

CREATE TABLE nature (
    id integer NOT NULL,
    name character varying(100),
    description text
);

COMMENT ON TABLE nature IS 'Список возможной природы объектов исследований.';

COMMENT ON COLUMN nature.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN nature.name IS 'Название.';

COMMENT ON COLUMN nature.description IS 'Описание.';

CREATE SEQUENCE nature_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE nature_id_seq OWNED BY nature.id;

CREATE TABLE notation (
    id integer NOT NULL,
    name character varying(100),
    description text,
    nature_id integer NOT NULL
);

COMMENT ON TABLE notation IS 'таблица с перечнем форм записи цепочек.';

COMMENT ON COLUMN notation.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN notation.name IS 'Название.';

COMMENT ON COLUMN notation.description IS 'Описание.';

COMMENT ON COLUMN notation.nature_id IS 'Соответствующая данной форме записи природа объекта.';

CREATE SEQUENCE notation_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE notation_id_seq OWNED BY notation.id;

CREATE TABLE note (
    notation_id integer DEFAULT 8,
    numerator integer NOT NULL,
    denominator integer NOT NULL,
    ticks integer,
    onumerator integer NOT NULL,
    odenominator integer NOT NULL,
    triplet boolean NOT NULL,
    priority integer,
    tie_id integer NOT NULL
)
INHERITS (element);

COMMENT ON TABLE note IS 'Таблица элементов-нот.';

COMMENT ON COLUMN note.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN note.value IS 'Нота в виде строки.';

COMMENT ON COLUMN note.description IS 'Описание.';

COMMENT ON COLUMN note.name IS 'Название.';

COMMENT ON COLUMN note.notation_id IS 'Форма записи. Рудиментное поле, которое всегда принимает значение 8.';

COMMENT ON COLUMN note.created IS 'Дата создания.';

COMMENT ON COLUMN note.modified IS 'Дата и время последнего изменения записи в таблице.';

COMMENT ON COLUMN note.numerator IS 'Числитель в дроби доли.';

COMMENT ON COLUMN note.denominator IS 'Знаменатель в дроби доли.';

COMMENT ON COLUMN note.ticks IS 'Количество МИДИ тиков в доле.';

COMMENT ON COLUMN note.onumerator IS 'Оригинальный числитель в дроби доли (для сохранения после наложения триоли на длительность).';

COMMENT ON COLUMN note.odenominator IS ' Оригинальный знаменатель в дроби доли(для сохранения после наложения триоли на длительность).';

COMMENT ON COLUMN note.triplet IS 'Флаг наличия триоли.';

COMMENT ON COLUMN note.priority IS 'Ритмический приоритет.';

COMMENT ON COLUMN note.tie_id IS 'Лига.';

CREATE TABLE note_symbol (
    id integer NOT NULL,
    name character varying(100),
    description text
);

COMMENT ON TABLE note_symbol IS 'Справочная таблица буквенных обозначений нот.';

COMMENT ON COLUMN note_symbol.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN note_symbol.name IS 'Название.';

COMMENT ON COLUMN note_symbol.description IS 'Описание.';

CREATE SEQUENCE note_symbol_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE note_symbol_id_seq OWNED BY note_symbol.id;

CREATE TABLE piece (
    id bigint NOT NULL,
    gene_id bigint NOT NULL,
    start integer NOT NULL,
    length integer NOT NULL
);

COMMENT ON TABLE piece IS 'Таблица с позициями фрагментов генов.';

COMMENT ON COLUMN piece.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN piece.gene_id IS 'Принадлежит гену.';

COMMENT ON COLUMN piece.start IS 'Индекс начала фрагмента.';

COMMENT ON COLUMN piece.length IS 'Длина фрагмента.';

CREATE SEQUENCE piece_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE piece_id_seq OWNED BY piece.id;

CREATE TABLE piece_type (
    id integer NOT NULL,
    name character varying(100),
    description text,
    nature_id integer NOT NULL
);

COMMENT ON TABLE piece_type IS 'Справочная таблица с типами фрагментов цепочек.';

COMMENT ON COLUMN piece_type.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN piece_type.name IS 'Название.';

COMMENT ON COLUMN piece_type.description IS 'Описание.';

COMMENT ON COLUMN piece_type.nature_id IS 'Природа фрагмента.';

CREATE SEQUENCE piece_type_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE piece_type_id_seq OWNED BY piece_type.id;

CREATE TABLE pitch (
    id integer NOT NULL,
    octave integer NOT NULL,
    midinumber integer NOT NULL,
    instrument_id integer NOT NULL,
    note_id bigint NOT NULL,
    accidental_id integer NOT NULL,
    note_symbol_id integer NOT NULL,
    created timestamp with time zone DEFAULT now() NOT NULL,
    modified timestamp with time zone DEFAULT now() NOT NULL
);

COMMENT ON TABLE pitch IS 'Высота ноты.';

COMMENT ON COLUMN pitch.id IS 'Уникальный внутренний идентификатор таблицы pitch.';

COMMENT ON COLUMN pitch.octave IS 'Номер октавы.';

COMMENT ON COLUMN pitch.midinumber IS 'Уникальный номер ноты по миди стандарту.';

COMMENT ON COLUMN pitch.instrument_id IS 'Номер музыкального инструмента.';

COMMENT ON COLUMN pitch.note_id IS 'Ссылка на ноту, которой принадлежит высота.';

COMMENT ON COLUMN pitch.accidental_id IS 'Знак альтерации.';

COMMENT ON COLUMN pitch.note_symbol_id IS 'Символ ноты.';

COMMENT ON COLUMN pitch.created IS 'Дата и время создания высоты ноты.';

COMMENT ON COLUMN pitch.modified IS 'Дата и время последнего изменения записи в таблице.';

CREATE SEQUENCE pitch_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE pitch_id_seq OWNED BY pitch.id;

CREATE TABLE product (
    id integer NOT NULL,
    name character varying(255),
    description text,
    piece_type_id integer NOT NULL
);

COMMENT ON TABLE product IS 'Таблица с перечнем типов генетических последовательностей.';

COMMENT ON COLUMN product.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN product.name IS 'Название.';

COMMENT ON COLUMN product.description IS 'Описание.';

COMMENT ON COLUMN product.piece_type_id IS 'Соответствующий данному типу последовательности тип фрагмента (тРНК, рРНК, ген, и т.д.).';

CREATE SEQUENCE product_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE product_id_seq OWNED BY product.id;

CREATE TABLE remote_db (
    id integer NOT NULL,
    name character varying(100),
    description text,
    url character varying(255),
    nature_id integer NOT NULL
);

COMMENT ON TABLE remote_db IS 'Таблица со списком баз данных из которых брались цепочки.';

COMMENT ON COLUMN remote_db.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN remote_db.name IS 'Название БД.';

COMMENT ON COLUMN remote_db.description IS 'Описание или пояснения.';

COMMENT ON COLUMN remote_db.url IS 'URL сетевого ресурса БД.';

COMMENT ON COLUMN remote_db.nature_id IS 'Природа объектов хранимых в удалённой БД.';

CREATE SEQUENCE remote_db_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE remote_db_id_seq OWNED BY remote_db.id;

CREATE TABLE tie (
    id integer NOT NULL,
    name character varying(100),
    description text
);

COMMENT ON TABLE tie IS 'Справочная таблица лиг.';

COMMENT ON COLUMN tie.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN tie.name IS 'Название.';

COMMENT ON COLUMN tie.description IS 'Описание.';

CREATE SEQUENCE tie_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE tie_id_seq OWNED BY tie.id;

CREATE TABLE translator (
    id integer NOT NULL,
    name character varying(100),
    description text
);

COMMENT ON TABLE translator IS 'Справочная таблица переводчиков литературных произведений.';

COMMENT ON COLUMN translator.id IS 'Уникальный внутренний идентификатор.';

COMMENT ON COLUMN translator.name IS 'Название привязки.';

COMMENT ON COLUMN translator.description IS 'Описание привязки.';

CREATE SEQUENCE translator_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE translator_id_seq OWNED BY translator.id;

ALTER TABLE ONLY accidental ALTER COLUMN id SET DEFAULT nextval('accidental_id_seq'::regclass);

ALTER TABLE ONLY binary_characteristic ALTER COLUMN id SET DEFAULT nextval('binary_characteristic_id_seq'::regclass);

ALTER TABLE ONLY characteristic ALTER COLUMN id SET DEFAULT nextval('characteristics_id_seq'::regclass);

ALTER TABLE ONLY characteristic_group ALTER COLUMN id SET DEFAULT nextval('characteristic_group_id_seq'::regclass);

ALTER TABLE ONLY characteristic_type ALTER COLUMN id SET DEFAULT nextval('characteristic_type_id_seq'::regclass);

ALTER TABLE ONLY congeneric_characteristic ALTER COLUMN id SET DEFAULT nextval('congeneric_characteristic_id_seq'::regclass);

ALTER TABLE ONLY dna_chain ALTER COLUMN id SET DEFAULT nextval('elements_id_seq'::regclass);

ALTER TABLE ONLY dna_chain ALTER COLUMN created SET DEFAULT now();

ALTER TABLE ONLY dna_chain ALTER COLUMN dissimilar SET DEFAULT false;

ALTER TABLE ONLY dna_chain ALTER COLUMN piece_type_id SET DEFAULT 1;

ALTER TABLE ONLY dna_chain ALTER COLUMN piece_position SET DEFAULT 0;

ALTER TABLE ONLY dna_chain ALTER COLUMN remote_db_id SET DEFAULT 1;

ALTER TABLE ONLY dna_chain ALTER COLUMN modified SET DEFAULT now();

ALTER TABLE ONLY element ALTER COLUMN id SET DEFAULT nextval('elements_id_seq'::regclass);

ALTER TABLE ONLY fmotiv ALTER COLUMN created SET DEFAULT now();

ALTER TABLE ONLY fmotiv ALTER COLUMN dissimilar SET DEFAULT false;

ALTER TABLE ONLY fmotiv ALTER COLUMN piece_type_id SET DEFAULT 1;

ALTER TABLE ONLY fmotiv ALTER COLUMN piece_position SET DEFAULT 0;

ALTER TABLE ONLY fmotiv ALTER COLUMN modified SET DEFAULT now();

ALTER TABLE ONLY fmotiv_type ALTER COLUMN id SET DEFAULT nextval('fmotiv_type_id_seq'::regclass);

ALTER TABLE ONLY instrument ALTER COLUMN id SET DEFAULT nextval('instrument_id_seq'::regclass);

ALTER TABLE ONLY language ALTER COLUMN id SET DEFAULT nextval('language_id_seq'::regclass);

ALTER TABLE ONLY link ALTER COLUMN id SET DEFAULT nextval('link_id_seq'::regclass);

ALTER TABLE ONLY literature_chain ALTER COLUMN id SET DEFAULT nextval('elements_id_seq'::regclass);

ALTER TABLE ONLY literature_chain ALTER COLUMN created SET DEFAULT now();

ALTER TABLE ONLY literature_chain ALTER COLUMN dissimilar SET DEFAULT false;

ALTER TABLE ONLY literature_chain ALTER COLUMN piece_type_id SET DEFAULT 1;

ALTER TABLE ONLY literature_chain ALTER COLUMN piece_position SET DEFAULT 0;

ALTER TABLE ONLY literature_chain ALTER COLUMN modified SET DEFAULT now();

ALTER TABLE ONLY matter ALTER COLUMN id SET DEFAULT nextval('matter_id_seq'::regclass);

ALTER TABLE ONLY measure ALTER COLUMN created SET DEFAULT now();

ALTER TABLE ONLY measure ALTER COLUMN dissimilar SET DEFAULT false;

ALTER TABLE ONLY measure ALTER COLUMN piece_type_id SET DEFAULT 1;

ALTER TABLE ONLY measure ALTER COLUMN piece_position SET DEFAULT 0;

ALTER TABLE ONLY measure ALTER COLUMN modified SET DEFAULT now();

ALTER TABLE ONLY music_chain ALTER COLUMN id SET DEFAULT nextval('elements_id_seq'::regclass);

ALTER TABLE ONLY music_chain ALTER COLUMN created SET DEFAULT now();

ALTER TABLE ONLY music_chain ALTER COLUMN dissimilar SET DEFAULT false;

ALTER TABLE ONLY music_chain ALTER COLUMN piece_type_id SET DEFAULT 1;

ALTER TABLE ONLY music_chain ALTER COLUMN piece_position SET DEFAULT 0;

ALTER TABLE ONLY music_chain ALTER COLUMN modified SET DEFAULT now();

ALTER TABLE ONLY nature ALTER COLUMN id SET DEFAULT nextval('nature_id_seq'::regclass);

ALTER TABLE ONLY notation ALTER COLUMN id SET DEFAULT nextval('notation_id_seq'::regclass);

ALTER TABLE ONLY note ALTER COLUMN id SET DEFAULT nextval('elements_id_seq'::regclass);

ALTER TABLE ONLY note ALTER COLUMN created SET DEFAULT now();

ALTER TABLE ONLY note ALTER COLUMN modified SET DEFAULT now();

ALTER TABLE ONLY note_symbol ALTER COLUMN id SET DEFAULT nextval('note_symbol_id_seq'::regclass);

ALTER TABLE ONLY piece ALTER COLUMN id SET DEFAULT nextval('piece_id_seq'::regclass);

ALTER TABLE ONLY piece_type ALTER COLUMN id SET DEFAULT nextval('piece_type_id_seq'::regclass);

ALTER TABLE ONLY pitch ALTER COLUMN id SET DEFAULT nextval('pitch_id_seq'::regclass);

ALTER TABLE ONLY product ALTER COLUMN id SET DEFAULT nextval('product_id_seq'::regclass);

ALTER TABLE ONLY remote_db ALTER COLUMN id SET DEFAULT nextval('remote_db_id_seq'::regclass);

ALTER TABLE ONLY tie ALTER COLUMN id SET DEFAULT nextval('tie_id_seq'::regclass);

ALTER TABLE ONLY translator ALTER COLUMN id SET DEFAULT nextval('translator_id_seq'::regclass);

ALTER TABLE ONLY accidental
    ADD CONSTRAINT pk_accidental PRIMARY KEY (id);

ALTER TABLE ONLY binary_characteristic
    ADD CONSTRAINT pk_binary_characteristic PRIMARY KEY (id);

ALTER TABLE ONLY chain
    ADD CONSTRAINT pk_chain PRIMARY KEY (id);

ALTER TABLE ONLY chain_key
    ADD CONSTRAINT pk_chain_key PRIMARY KEY (id);

ALTER TABLE ONLY characteristic
    ADD CONSTRAINT pk_characteristic PRIMARY KEY (id);

ALTER TABLE ONLY characteristic_group
    ADD CONSTRAINT pk_characteristic_groups PRIMARY KEY (id);

ALTER TABLE ONLY characteristic_type
    ADD CONSTRAINT pk_characteristic_types PRIMARY KEY (id);

ALTER TABLE ONLY congeneric_characteristic
    ADD CONSTRAINT pk_congeneric_characteristic PRIMARY KEY (id);

ALTER TABLE ONLY dna_chain
    ADD CONSTRAINT pk_dna_chain PRIMARY KEY (id);

ALTER TABLE ONLY element_key
    ADD CONSTRAINT pk_element_key PRIMARY KEY (id);

ALTER TABLE ONLY element
    ADD CONSTRAINT pk_elements PRIMARY KEY (id);

ALTER TABLE ONLY fmotiv
    ADD CONSTRAINT pk_fmotiv PRIMARY KEY (id);

ALTER TABLE ONLY fmotiv_type
    ADD CONSTRAINT pk_fmotiv_type PRIMARY KEY (id);

ALTER TABLE ONLY gene
    ADD CONSTRAINT pk_gene PRIMARY KEY (id);

ALTER TABLE ONLY instrument
    ADD CONSTRAINT pk_instrument PRIMARY KEY (id);

ALTER TABLE ONLY language
    ADD CONSTRAINT pk_language PRIMARY KEY (id);

ALTER TABLE ONLY link
    ADD CONSTRAINT pk_link PRIMARY KEY (id);

ALTER TABLE ONLY literature_chain
    ADD CONSTRAINT pk_literature_chain PRIMARY KEY (id);

ALTER TABLE ONLY matter
    ADD CONSTRAINT pk_matter PRIMARY KEY (id);

ALTER TABLE ONLY measure
    ADD CONSTRAINT pk_measure PRIMARY KEY (id);

ALTER TABLE ONLY music_chain
    ADD CONSTRAINT pk_music_chain PRIMARY KEY (id);

ALTER TABLE ONLY nature
    ADD CONSTRAINT pk_nature PRIMARY KEY (id);

ALTER TABLE ONLY notation
    ADD CONSTRAINT pk_notation PRIMARY KEY (id);

ALTER TABLE ONLY note
    ADD CONSTRAINT pk_note PRIMARY KEY (id);

ALTER TABLE ONLY note_symbol
    ADD CONSTRAINT pk_note_symbol PRIMARY KEY (id);

ALTER TABLE ONLY piece
    ADD CONSTRAINT pk_piece PRIMARY KEY (id);

ALTER TABLE ONLY piece_type
    ADD CONSTRAINT pk_piece_type PRIMARY KEY (id);

ALTER TABLE ONLY pitch
    ADD CONSTRAINT pk_pitch PRIMARY KEY (id);

ALTER TABLE ONLY product
    ADD CONSTRAINT pk_product PRIMARY KEY (id);

ALTER TABLE ONLY remote_db
    ADD CONSTRAINT pk_remote_db PRIMARY KEY (id);

ALTER TABLE ONLY tie
    ADD CONSTRAINT pk_tie PRIMARY KEY (id);

ALTER TABLE ONLY translator
    ADD CONSTRAINT pk_translator PRIMARY KEY (id);

ALTER TABLE ONLY accidental
    ADD CONSTRAINT uk_accidental_name UNIQUE (name);

ALTER TABLE ONLY characteristic_group
    ADD CONSTRAINT uk_characteristic_group_name UNIQUE (name);

ALTER TABLE ONLY characteristic_type
    ADD CONSTRAINT uk_characteristic_type_name UNIQUE (name);

ALTER TABLE ONLY dna_chain
    ADD CONSTRAINT uk_dna_chain UNIQUE (matter_id, notation_id, piece_type_id, piece_position);

ALTER TABLE ONLY element
    ADD CONSTRAINT uk_element_value_notation UNIQUE (value, notation_id);

ALTER TABLE ONLY fmotiv_type
    ADD CONSTRAINT uk_fmotiv_type_name UNIQUE (name);

ALTER TABLE ONLY instrument
    ADD CONSTRAINT uk_instrument_name UNIQUE (name);

ALTER TABLE ONLY language
    ADD CONSTRAINT uk_language_name UNIQUE (name);

ALTER TABLE ONLY link
    ADD CONSTRAINT uk_link_name UNIQUE (name);

ALTER TABLE ONLY matter
    ADD CONSTRAINT uk_matter UNIQUE (name, nature_id);

ALTER TABLE ONLY nature
    ADD CONSTRAINT uk_nature_name UNIQUE (name);

ALTER TABLE ONLY notation
    ADD CONSTRAINT uk_notation_name UNIQUE (name);

ALTER TABLE ONLY note_symbol
    ADD CONSTRAINT uk_note_symbol_name UNIQUE (name);

ALTER TABLE ONLY piece
    ADD CONSTRAINT uk_piece UNIQUE (gene_id, start, length);

ALTER TABLE ONLY piece_type
    ADD CONSTRAINT uk_piece_type UNIQUE (name);

ALTER TABLE ONLY product
    ADD CONSTRAINT uk_product_name UNIQUE (name);

ALTER TABLE ONLY remote_db
    ADD CONSTRAINT uk_remote_db_name UNIQUE (name);

ALTER TABLE ONLY remote_db
    ADD CONSTRAINT uk_remote_db_url UNIQUE (url);

ALTER TABLE ONLY tie
    ADD CONSTRAINT uk_tie_name UNIQUE (name);

ALTER TABLE ONLY translator
    ADD CONSTRAINT uk_translator_name UNIQUE (name);

CREATE INDEX fki_congeneric_characteristic_alphabet_element ON congeneric_characteristic USING btree (chain_id, element_id);

CREATE INDEX "ix-matter_name_nature" ON matter USING btree (name, nature_id);

COMMENT ON INDEX "ix-matter_name_nature" IS 'Индекс по именам объектов исследования.';

CREATE INDEX ix_accidental_id ON accidental USING btree (id);

CREATE INDEX ix_accidental_name ON accidental USING btree (name);

CREATE INDEX ix_binary_characteristic_chain_id ON binary_characteristic USING btree (chain_id);

COMMENT ON INDEX ix_binary_characteristic_chain_id IS 'Индекс бинарных характеристик по цепочкам.';

CREATE INDEX ix_binary_characteristic_chain_link_characteristic_type ON binary_characteristic USING btree (chain_id, characteristic_type_id, link_id);

COMMENT ON INDEX ix_binary_characteristic_chain_link_characteristic_type IS 'Индекс для выбора характеристики определённой цепочки с определённой привязкой.';

CREATE INDEX ix_binary_characteristic_created ON binary_characteristic USING btree (created);

COMMENT ON INDEX ix_binary_characteristic_created IS 'Индекс характерисктик по датам их вычисления.';

CREATE INDEX ix_chain_alphabet ON chain USING gin (alphabet);

CREATE INDEX ix_chain_id ON chain USING btree (id);

COMMENT ON INDEX ix_chain_id IS 'Индекс id цепочек.';

CREATE INDEX ix_chain_key ON chain_key USING btree (id);

CREATE INDEX ix_chain_matter_id ON chain USING btree (matter_id);

COMMENT ON INDEX ix_chain_matter_id IS 'Индекс по объектам исследования которым принадлежат цепочки.';

CREATE INDEX ix_chain_notation_id ON chain USING btree (notation_id);

COMMENT ON INDEX ix_chain_notation_id IS 'Индекс по формам записи цепочек.';

CREATE INDEX ix_chain_piece_type_id ON chain USING btree (piece_type_id);

COMMENT ON INDEX ix_chain_piece_type_id IS 'Индекс по типам частей цепочек.';

CREATE INDEX ix_characteristic_chain_id ON characteristic USING btree (chain_id);

COMMENT ON INDEX ix_characteristic_chain_id IS 'Индекс характерисктик по цепочкам.';

CREATE INDEX ix_characteristic_chain_link_characteristic_type ON characteristic USING btree (chain_id, link_id, characteristic_type_id);

COMMENT ON INDEX ix_characteristic_chain_link_characteristic_type IS 'Индекс по значениям характеристик.';

CREATE INDEX ix_characteristic_chracteristic_type_id ON characteristic USING btree (characteristic_type_id);

COMMENT ON INDEX ix_characteristic_chracteristic_type_id IS 'Индекс характеристик по типам.';

CREATE INDEX ix_characteristic_created ON characteristic USING btree (created);

COMMENT ON INDEX ix_characteristic_created IS 'Индекс характерисктик по датам их вычисления.';

CREATE INDEX ix_characteristic_group_id ON characteristic_group USING btree (id);

COMMENT ON INDEX ix_characteristic_group_id IS 'Индекс по первичному ключу таблицы characteristic_group.';

CREATE INDEX ix_characteristic_group_name ON characteristic_group USING btree (name);

COMMENT ON INDEX ix_characteristic_group_name IS 'Индекс по названию групп характеристик.';

CREATE INDEX ix_characteristic_id ON characteristic USING btree (id);

COMMENT ON INDEX ix_characteristic_id IS 'Индекс по первичному ключу таблицы characteristic.';

CREATE INDEX ix_characteristic_type_characteristic_group ON characteristic_type USING btree (characteristic_group_id);

COMMENT ON INDEX ix_characteristic_type_characteristic_group IS 'Индекс по группам характеристик.';

CREATE INDEX ix_characteristic_type_id ON characteristic_type USING btree (id);

COMMENT ON INDEX ix_characteristic_type_id IS 'Индекс попервичному ключу таблицы characteristic_type.';

CREATE INDEX ix_characteristic_type_name ON characteristic_type USING btree (name);

COMMENT ON INDEX ix_characteristic_type_name IS 'Индекс по названиям характеристик.';

CREATE INDEX ix_congeneric_characteristic_chain_characterisric_link_element ON congeneric_characteristic USING btree (chain_id, characteristic_type_id, link_id, element_id);

COMMENT ON INDEX ix_congeneric_characteristic_chain_characterisric_link_element IS 'Индекс для поиска определённой характеристики определённой цепочки.';

CREATE INDEX ix_congeneric_characteristic_chain_id ON congeneric_characteristic USING btree (chain_id);

CREATE INDEX ix_congeneric_characteristic_chain_link_characteristic_type ON congeneric_characteristic USING btree (chain_id, link_id, characteristic_type_id);

COMMENT ON INDEX ix_congeneric_characteristic_chain_link_characteristic_type IS 'Индекс по значениям характеристик однородных цепочек.';

CREATE INDEX ix_congeneric_characteristic_created ON congeneric_characteristic USING btree (created);

COMMENT ON INDEX ix_congeneric_characteristic_created IS 'Индекс характерисктик по датам их вычисления.';

CREATE INDEX ix_dna_chain_alphabet ON dna_chain USING gin (alphabet);

CREATE INDEX ix_dna_chain_id ON dna_chain USING btree (id);

COMMENT ON INDEX ix_dna_chain_id IS 'Индекс id цепочек.';

CREATE INDEX ix_dna_chain_matter_id ON dna_chain USING btree (matter_id);

COMMENT ON INDEX ix_dna_chain_matter_id IS 'Индекс по объектам исследования которым принадлежат цепчоки ДНК.';

CREATE INDEX ix_dna_chain_notation_id ON dna_chain USING btree (notation_id);

COMMENT ON INDEX ix_dna_chain_notation_id IS 'Индекс по форме записи цепочек ДНК.';

CREATE INDEX ix_dna_chain_piece_type_id ON dna_chain USING btree (piece_type_id);

COMMENT ON INDEX ix_dna_chain_piece_type_id IS 'Индекс по типу фрагмента цепочек ДНК.';

CREATE INDEX ix_element_id ON element USING btree (id);

COMMENT ON INDEX ix_element_id IS 'Индекс по первичному ключу таблицы element.';

CREATE INDEX ix_element_key ON element_key USING btree (id);

CREATE INDEX ix_element_notation_id ON element USING btree (notation_id);

COMMENT ON INDEX ix_element_notation_id IS 'Индекс элементов по форме записи.';

CREATE INDEX ix_fmotiv_alphabet ON fmotiv USING gin (alphabet);

CREATE INDEX ix_fmotiv_id ON fmotiv USING btree (id);

CREATE INDEX ix_fmotiv_matter_id ON fmotiv USING btree (matter_id);

CREATE INDEX ix_fmotiv_notation_id ON fmotiv USING btree (notation_id);

CREATE INDEX ix_fmotiv_piece_type_id ON fmotiv USING btree (piece_type_id);

CREATE INDEX ix_fmotiv_type_id ON fmotiv_type USING btree (id);

CREATE INDEX ix_fmotiv_type_name ON fmotiv_type USING btree (name);

CREATE INDEX ix_gene_chain_id ON gene USING btree (chain_id);

COMMENT ON INDEX ix_gene_chain_id IS 'Индекс по цепочкам которым принадлежат цепчоки ДНК.';

CREATE INDEX ix_gene_id ON gene USING btree (id);

COMMENT ON INDEX ix_gene_id IS 'Индекс id генов.';

CREATE INDEX ix_gene_piece_type_id ON gene USING btree (piece_type_id);

COMMENT ON INDEX ix_gene_piece_type_id IS 'Индекс по типу фрагмента цепочек ДНК.';

CREATE INDEX ix_instrument_id ON instrument USING btree (id);

CREATE INDEX ix_instrument_name ON instrument USING btree (name);

CREATE INDEX ix_language_id ON language USING btree (id);

CREATE INDEX ix_language_name ON language USING btree (name);

CREATE INDEX ix_link_id ON link USING btree (id);

COMMENT ON INDEX ix_link_id IS 'Индекс первичного ключа таблицы link.';

CREATE INDEX ix_link_name ON link USING btree (name);

COMMENT ON INDEX ix_link_name IS 'Индекс по именам привязок.';

CREATE INDEX ix_literature_chain_alphabet ON literature_chain USING gin (alphabet);

CREATE INDEX ix_literature_chain_id ON literature_chain USING btree (id);

CREATE INDEX ix_literature_chain_matter_id ON literature_chain USING btree (matter_id);

CREATE INDEX ix_literature_chain_matter_language ON literature_chain USING btree (matter_id, language_id);

CREATE INDEX ix_literature_chain_notation_id ON literature_chain USING btree (notation_id);

CREATE INDEX ix_literature_chain_piece_type_id ON literature_chain USING btree (piece_type_id);

CREATE INDEX ix_matter_matter_id ON matter USING btree (id);

COMMENT ON INDEX ix_matter_matter_id IS 'Индекс по первичному ключу таблицы matter.';

CREATE INDEX ix_matter_nature ON matter USING btree (nature_id);

COMMENT ON INDEX ix_matter_nature IS 'Индекс по природе таблицы matter.';

CREATE INDEX ix_measure_alphabet ON measure USING gin (alphabet);

CREATE INDEX ix_measure_id ON measure USING btree (id);

CREATE INDEX ix_measure_matter_id ON measure USING btree (matter_id);

CREATE INDEX ix_measure_notation_id ON measure USING btree (notation_id);

CREATE INDEX ix_measure_piece_type_id ON measure USING btree (piece_type_id);

CREATE INDEX ix_music_chain_alphabet ON music_chain USING gin (alphabet);

CREATE INDEX ix_music_chain_id ON music_chain USING btree (id);

CREATE INDEX ix_music_chain_matter_id ON music_chain USING btree (matter_id);

CREATE INDEX ix_music_chain_notation_id ON music_chain USING btree (notation_id);

CREATE INDEX ix_music_chain_piece_type_id ON music_chain USING btree (piece_type_id);

CREATE INDEX ix_nature_id ON nature USING btree (id);

COMMENT ON INDEX ix_nature_id IS 'Индекс первичного ключа таблицы nature.';

CREATE INDEX ix_nature_name ON nature USING btree (name);

COMMENT ON INDEX ix_nature_name IS 'Индекс по имени таблицы nature.';

CREATE INDEX ix_notation_id ON notation USING btree (id);

COMMENT ON INDEX ix_notation_id IS 'Индкс первичного ключа таблицы notation.';

CREATE INDEX ix_notation_nature ON notation USING btree (nature_id);

COMMENT ON INDEX ix_notation_nature IS 'Индекс внешнего ключа таблицы notation.';

CREATE INDEX ix_note_id ON note USING btree (id);

CREATE INDEX ix_note_notation_id ON note USING btree (notation_id);

CREATE INDEX ix_note_symbol_id ON note_symbol USING btree (id);

CREATE INDEX ix_note_symbol_name ON note_symbol USING btree (name);

CREATE INDEX ix_piece_type_id ON piece_type USING btree (id);

COMMENT ON INDEX ix_piece_type_id IS 'Индекс первичного ключа таблицы piece_type.';

CREATE INDEX ix_piece_type_name ON piece_type USING btree (name);

CREATE INDEX ix_piece_type_nature ON piece_type USING btree (nature_id);

COMMENT ON INDEX ix_piece_type_nature IS 'Индекс внешнего ключа таблицы piece_type.';

CREATE INDEX ix_pitch ON pitch USING btree (id);

CREATE INDEX ix_pitch_note ON pitch USING btree (note_id);

CREATE INDEX ix_product_id ON product USING btree (id);

COMMENT ON INDEX ix_product_id IS 'Индкс первичного ключа таблицы product.';

CREATE INDEX ix_product_piece_type ON product USING btree (piece_type_id);

COMMENT ON INDEX ix_product_piece_type IS 'Индекс внешнего ключа таблицы product.';

CREATE INDEX ix_remote_db_id ON remote_db USING btree (id);

COMMENT ON INDEX ix_remote_db_id IS 'Индекс первичного ключа таблицы remote_db.';

CREATE INDEX ix_remote_db_name ON remote_db USING btree (name);

COMMENT ON INDEX ix_remote_db_name IS 'Индекс по имени удалённой базы данных.';

CREATE INDEX ix_tie_id ON tie USING btree (id);

CREATE INDEX ix_tie_name ON tie USING btree (name);

CREATE INDEX ix_translator_id ON translator USING btree (id);

COMMENT ON INDEX ix_translator_id IS 'Индекс первичного ключа таблицы translator.';

CREATE INDEX ix_translator_name ON translator USING btree (name);

COMMENT ON INDEX ix_translator_name IS 'Индекс по именам переводчиков.';

CREATE UNIQUE INDEX uk_binary_characteristic_value_link_not_null ON binary_characteristic USING btree (chain_id, characteristic_type_id, link_id, first_element_id, second_element_id) WHERE (link_id IS NOT NULL);

CREATE UNIQUE INDEX uk_binary_characteristic_value_link_null ON binary_characteristic USING btree (chain_id, characteristic_type_id, first_element_id, second_element_id) WHERE (link_id IS NULL);

CREATE UNIQUE INDEX uk_characteristic_value_link_not_null ON characteristic USING btree (chain_id, characteristic_type_id, link_id) WHERE (link_id IS NOT NULL);

CREATE UNIQUE INDEX uk_characteristic_value_link_null ON characteristic USING btree (chain_id, characteristic_type_id) WHERE (link_id IS NULL);

CREATE UNIQUE INDEX uk_congeneric_characteristic_link_not_null ON congeneric_characteristic USING btree (chain_id, characteristic_type_id, link_id, element_id) WHERE (link_id IS NOT NULL);

CREATE UNIQUE INDEX uk_congeneric_characteristic_link_null ON congeneric_characteristic USING btree (chain_id, characteristic_type_id, element_id) WHERE (link_id IS NULL);

CREATE UNIQUE INDEX uk_literature_chain_translator_not_null ON literature_chain USING btree (notation_id, matter_id, piece_type_id, piece_position, language_id, translator_id) WHERE (translator_id IS NOT NULL);

CREATE UNIQUE INDEX uk_literature_chain_translator_null ON literature_chain USING btree (notation_id, matter_id, piece_type_id, piece_position, language_id) WHERE (translator_id IS NULL);

CREATE TRIGGER tgd_element_key BEFORE DELETE ON element_key FOR EACH ROW EXECUTE PROCEDURE trigger_element_delete_alphabet_bound();

COMMENT ON TRIGGER tgd_element_key ON element_key IS 'Проверяет, не используется ли удаляемый элемент в каком-либо алфавите.';

CREATE TRIGGER tgi_chain_building_check BEFORE INSERT ON chain FOR EACH ROW EXECUTE PROCEDURE trigger_building_check();

COMMENT ON TRIGGER tgi_chain_building_check ON chain IS 'Триггер, проверяющий строй цепочки.';

CREATE TRIGGER tgi_chain_key BEFORE INSERT ON chain_key FOR EACH ROW EXECUTE PROCEDURE trigger_chain_key_insert();

CREATE TRIGGER tgi_dna_chain_building_check BEFORE INSERT ON dna_chain FOR EACH ROW EXECUTE PROCEDURE trigger_building_check();

COMMENT ON TRIGGER tgi_dna_chain_building_check ON dna_chain IS 'Триггер, проверяющий строй цепочки.';

CREATE TRIGGER tgi_element_key BEFORE INSERT ON element_key FOR EACH ROW EXECUTE PROCEDURE trigger_element_key_insert();

CREATE TRIGGER tgi_fmotiv_building_check BEFORE INSERT ON fmotiv FOR EACH ROW EXECUTE PROCEDURE trigger_building_check();

COMMENT ON TRIGGER tgi_fmotiv_building_check ON fmotiv IS 'Триггер, проверяющий строй цепочки.';

CREATE TRIGGER tgi_literature_chain_building_check BEFORE INSERT ON literature_chain FOR EACH ROW EXECUTE PROCEDURE trigger_building_check();

COMMENT ON TRIGGER tgi_literature_chain_building_check ON literature_chain IS 'Триггер, проверяющий строй цепочки.';

CREATE TRIGGER tgi_measure_building_check BEFORE INSERT ON measure FOR EACH ROW EXECUTE PROCEDURE trigger_building_check();

COMMENT ON TRIGGER tgi_measure_building_check ON measure IS 'Триггер, проверяющий строй цепочки.';

CREATE TRIGGER tgi_music_chain_building_check BEFORE INSERT ON music_chain FOR EACH ROW EXECUTE PROCEDURE trigger_building_check();

COMMENT ON TRIGGER tgi_music_chain_building_check ON music_chain IS 'Триггер, проверяющий строй цепочки.';

CREATE TRIGGER tgiu_binary_characteristic_applicability BEFORE INSERT OR UPDATE OF characteristic_type_id ON binary_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_check_applicability('binary_chain_applicable');

COMMENT ON TRIGGER tgiu_binary_characteristic_applicability ON binary_characteristic IS 'Триггер, проверяющий применима ли указанная характеристика к бинарным цепочкам.';

CREATE TRIGGER tgiu_binary_characteristic_link BEFORE INSERT OR UPDATE OF characteristic_type_id, link_id ON binary_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_characteristics_link();

COMMENT ON TRIGGER tgiu_binary_characteristic_link ON binary_characteristic IS 'Триггер, проверяющий правильность привязки.';

CREATE TRIGGER tgiu_binary_characteristic_modified BEFORE INSERT OR UPDATE ON binary_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_binary_characteristic_modified ON binary_characteristic IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_binary_chracteristic_elements_in_alphabet BEFORE INSERT OR UPDATE OF first_element_id, second_element_id, chain_id ON binary_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_check_elements_in_alphabet();

COMMENT ON TRIGGER tgiu_binary_chracteristic_elements_in_alphabet ON binary_characteristic IS 'Триггер, проверяющий что оба элемента связываемые коэффициентом зависимости присутствуют в алфавите данной цепочки.';

CREATE TRIGGER tgiu_chain_alphabet AFTER INSERT OR UPDATE OF alphabet ON chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_check_alphabet();

COMMENT ON TRIGGER tgiu_chain_alphabet ON chain IS 'Проверяет наличие всех элементов добавляемого или изменяемого алфавита в БД.';

CREATE TRIGGER tgiu_chain_modified BEFORE INSERT OR UPDATE ON chain FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_chain_modified ON chain IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_characteristic_applicability BEFORE INSERT OR UPDATE OF characteristic_type_id ON characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_check_applicability('full_chain_applicable');

COMMENT ON TRIGGER tgiu_characteristic_applicability ON characteristic IS 'Триггер, проверяющий применима ли указанная характеристика к полным цепочкам.';

CREATE TRIGGER tgiu_characteristic_link BEFORE INSERT OR UPDATE OF characteristic_type_id, link_id ON characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_characteristics_link();

COMMENT ON TRIGGER tgiu_characteristic_link ON characteristic IS 'Триггер, проверяющий правильность привязки.';

CREATE TRIGGER tgiu_characteristic_modified BEFORE INSERT OR UPDATE ON characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_characteristic_modified ON characteristic IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_congeneric_characteristic_applicability BEFORE INSERT OR UPDATE OF characteristic_type_id ON congeneric_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_check_applicability('congeneric_chain_applicable');

COMMENT ON TRIGGER tgiu_congeneric_characteristic_applicability ON congeneric_characteristic IS 'Триггер, проверяющий применима ли указанная характеристика к однородным цепочкам.';

CREATE TRIGGER tgiu_congeneric_characteristic_link BEFORE INSERT OR UPDATE OF characteristic_type_id, link_id ON congeneric_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_characteristics_link();

COMMENT ON TRIGGER tgiu_congeneric_characteristic_link ON congeneric_characteristic IS 'Триггер, проверяющий правильность привязки.';

CREATE TRIGGER tgiu_congeneric_characteristic_modified BEFORE INSERT OR UPDATE ON congeneric_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_congeneric_characteristic_modified ON congeneric_characteristic IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_congeneric_chracteristic_element_in_alphabet BEFORE INSERT OR UPDATE OF element_id, chain_id ON congeneric_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_check_element_in_alphabet();

COMMENT ON TRIGGER tgiu_congeneric_chracteristic_element_in_alphabet ON congeneric_characteristic IS 'Триггер, проверяющий что элемент однородной цепочки, для которой вычислена характеристика, присутствует в алфавите данной цепочки.';

CREATE TRIGGER tgiu_dna_chain_alphabet AFTER INSERT OR UPDATE OF alphabet ON dna_chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_check_alphabet();

COMMENT ON TRIGGER tgiu_dna_chain_alphabet ON dna_chain IS 'Проверяет наличие всех элементов алфавита в БД.';

CREATE TRIGGER tgiu_dna_chain_modified BEFORE INSERT OR UPDATE ON dna_chain FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_dna_chain_modified ON dna_chain IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_element_modified BEFORE INSERT OR UPDATE ON element FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_element_modified ON element IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_fmotiv_alphabet AFTER INSERT OR UPDATE OF alphabet ON fmotiv FOR EACH STATEMENT EXECUTE PROCEDURE trigger_check_alphabet();

COMMENT ON TRIGGER tgiu_fmotiv_alphabet ON fmotiv IS 'Проверяет наличие всех элементов добавляемого или изменяемого алфавита в БД.';

CREATE TRIGGER tgiu_fmotiv_modified BEFORE INSERT OR UPDATE ON fmotiv FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_fmotiv_modified ON fmotiv IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_gene_modified BEFORE INSERT OR UPDATE ON gene FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_gene_modified ON gene IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_literature_chain_alphabet AFTER INSERT OR UPDATE OF alphabet ON literature_chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_check_alphabet();

COMMENT ON TRIGGER tgiu_literature_chain_alphabet ON literature_chain IS 'Проверяет наличие всех элементов добавляемого или изменяемого алфавита в БД.';

CREATE TRIGGER tgiu_literature_chain_modified BEFORE INSERT OR UPDATE ON literature_chain FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_literature_chain_modified ON literature_chain IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_matter_modified BEFORE INSERT OR UPDATE ON matter FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_matter_modified ON matter IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_measure_alphabet AFTER INSERT OR UPDATE OF alphabet ON measure FOR EACH STATEMENT EXECUTE PROCEDURE trigger_check_alphabet();

COMMENT ON TRIGGER tgiu_measure_alphabet ON measure IS 'Проверяет наличие всех элементов добавляемого или изменяемого алфавита в БД.';

CREATE TRIGGER tgiu_measure_modified BEFORE INSERT OR UPDATE ON measure FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_measure_modified ON measure IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_music_chain_alphabet AFTER INSERT OR UPDATE OF alphabet ON music_chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_check_alphabet();

COMMENT ON TRIGGER tgiu_music_chain_alphabet ON music_chain IS 'Проверяет наличие всех элементов добавляемого или изменяемого алфавита в БД.';

CREATE TRIGGER tgiu_music_chain_modified BEFORE INSERT OR UPDATE ON music_chain FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_music_chain_modified ON music_chain IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_note_modified BEFORE INSERT OR UPDATE ON note FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_note_modified ON note IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_pitch_modified BEFORE INSERT OR UPDATE ON pitch FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_pitch_modified ON pitch IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiud_chain_chain_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON chain FOR EACH ROW EXECUTE PROCEDURE trigger_chain_key_bound();

COMMENT ON TRIGGER tgiud_chain_chain_key_bound ON chain IS 'Дублирует добавление, изменение и удаление записей в таблице chain в таблицу chain_key.';

CREATE TRIGGER tgiud_dna_chain_chain_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON dna_chain FOR EACH ROW EXECUTE PROCEDURE trigger_chain_key_bound();

COMMENT ON TRIGGER tgiud_dna_chain_chain_key_bound ON dna_chain IS 'Дублирует добавление, изменение и удаление записей в таблице dna_chain в таблицу chain_key.';

CREATE TRIGGER tgiud_element_element_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON element FOR EACH ROW EXECUTE PROCEDURE trigger_element_key_bound();

COMMENT ON TRIGGER tgiud_element_element_key_bound ON element IS 'Дублирует добавление, изменение и удаление записей в таблице element в таблицу element_key.';

CREATE TRIGGER tgiud_fmotiv_chain_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON fmotiv FOR EACH ROW EXECUTE PROCEDURE trigger_chain_key_bound();

COMMENT ON TRIGGER tgiud_fmotiv_chain_key_bound ON fmotiv IS 'Дублирует добавление, изменение и удаление записей в таблице fmotiv в таблицу chain_key.';

CREATE TRIGGER tgiud_fmotiv_element_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON fmotiv FOR EACH ROW EXECUTE PROCEDURE trigger_element_key_bound();

COMMENT ON TRIGGER tgiud_fmotiv_element_key_bound ON fmotiv IS 'Дублирует добавление, изменение и удаление записей в таблице fmotiv в таблицу element_key.';

CREATE TRIGGER tgiud_gene_chain_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON gene FOR EACH ROW EXECUTE PROCEDURE trigger_chain_key_bound();

COMMENT ON TRIGGER tgiud_gene_chain_key_bound ON gene IS 'Дублирует добавление, изменение и удаление записей в таблице genes в таблицу chain_key.';

CREATE TRIGGER tgiud_literature_chain_chain_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON literature_chain FOR EACH ROW EXECUTE PROCEDURE trigger_chain_key_bound();

COMMENT ON TRIGGER tgiud_literature_chain_chain_key_bound ON literature_chain IS 'Дублирует добавление, изменение и удаление записей в таблице literature_chain в таблицу chain_key.';

CREATE TRIGGER tgiud_measure_chain_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON measure FOR EACH ROW EXECUTE PROCEDURE trigger_chain_key_bound();

COMMENT ON TRIGGER tgiud_measure_chain_key_bound ON measure IS 'Дублирует добавление, изменение и удаление записей в таблице measure в таблицу chain_key.';

CREATE TRIGGER tgiud_measure_element_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON measure FOR EACH ROW EXECUTE PROCEDURE trigger_element_key_bound();

COMMENT ON TRIGGER tgiud_measure_element_key_bound ON measure IS 'Дублирует добавление, изменение и удаление записей в таблице measure в таблицу element_key.';

CREATE TRIGGER tgiud_music_chain_chain_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON music_chain FOR EACH ROW EXECUTE PROCEDURE trigger_chain_key_bound();

COMMENT ON TRIGGER tgiud_music_chain_chain_key_bound ON music_chain IS 'Дублирует добавление, изменение и удаление записей в таблице music_chain в таблицу chain_key.';

CREATE TRIGGER tgiud_note_element_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON note FOR EACH ROW EXECUTE PROCEDURE trigger_element_key_bound();

COMMENT ON TRIGGER tgiud_note_element_key_bound ON note IS 'Дублирует добавление, изменение и удаление записей в таблице note в таблицу element_key.';

CREATE TRIGGER tgu_chain_characteristics AFTER UPDATE ON chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_chain_characteristics ON chain IS 'Триггер удаляющий все характеристки при обновлении цепочки.';

CREATE TRIGGER tgu_dna_chain_characteristics AFTER UPDATE ON dna_chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_dna_chain_characteristics ON dna_chain IS 'Триггер удаляющий все характеристки при обновлении цепочки.';

CREATE TRIGGER tgu_element_key AFTER UPDATE ON element_key FOR EACH ROW EXECUTE PROCEDURE trigger_element_update_alphabet();

COMMENT ON TRIGGER tgu_element_key ON element_key IS 'Триггер обновляющие все зависи в алфавтиах цепочек при перенумеровке элементов. Теоретически работает очень медленно, особенно при массовом перенумеровании.';

CREATE TRIGGER tgu_fmotiv_characteristics AFTER UPDATE ON fmotiv FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_fmotiv_characteristics ON fmotiv IS 'Триггер удаляющий все характеристки при обновлении цепочки.';

CREATE TRIGGER tgu_gene_characteristics AFTER UPDATE ON gene FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_gene_characteristics ON gene IS 'Триггер удаляющий все характеристки при обновлении цепочки.';

CREATE TRIGGER tgu_literature_chain_characteristics AFTER UPDATE ON literature_chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_literature_chain_characteristics ON literature_chain IS 'Триггер удаляющий все характеристки при обновлении цепочки.';

CREATE TRIGGER tgu_measure_characteristics AFTER UPDATE ON measure FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_measure_characteristics ON measure IS 'Триггер удаляющий все характеристки при обновлении цепочки.';

CREATE TRIGGER tgu_music_chain_characteristics AFTER UPDATE ON music_chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_music_chain_characteristics ON music_chain IS 'Триггер удаляющий все характеристки при обновлении цепочки.';

ALTER TABLE ONLY binary_characteristic
    ADD CONSTRAINT fk_binary_characteristic_chain_key FOREIGN KEY (chain_id) REFERENCES chain_key(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY binary_characteristic
    ADD CONSTRAINT fk_binary_characteristic_characteristic_type FOREIGN KEY (characteristic_type_id) REFERENCES characteristic_type(id) ON UPDATE CASCADE;

ALTER TABLE ONLY binary_characteristic
    ADD CONSTRAINT fk_binary_characteristic_element_key_first FOREIGN KEY (first_element_id) REFERENCES element_key(id) ON UPDATE CASCADE;

ALTER TABLE ONLY binary_characteristic
    ADD CONSTRAINT fk_binary_characteristic_element_key_second FOREIGN KEY (second_element_id) REFERENCES element_key(id) ON UPDATE CASCADE;

ALTER TABLE ONLY binary_characteristic
    ADD CONSTRAINT fk_binary_characteristic_link FOREIGN KEY (link_id) REFERENCES link(id) ON UPDATE CASCADE;

ALTER TABLE ONLY chain
    ADD CONSTRAINT fk_chain_chain_key FOREIGN KEY (id) REFERENCES chain_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY chain
    ADD CONSTRAINT fk_chain_matter FOREIGN KEY (matter_id) REFERENCES matter(id) ON UPDATE CASCADE;

ALTER TABLE ONLY chain
    ADD CONSTRAINT fk_chain_notation FOREIGN KEY (notation_id) REFERENCES notation(id) ON UPDATE CASCADE;

ALTER TABLE ONLY chain
    ADD CONSTRAINT fk_chain_piece_type FOREIGN KEY (piece_type_id) REFERENCES piece_type(id) ON UPDATE CASCADE;

ALTER TABLE ONLY chain
    ADD CONSTRAINT fk_chain_remote_db FOREIGN KEY (remote_db_id) REFERENCES remote_db(id) ON UPDATE CASCADE;

ALTER TABLE ONLY characteristic
    ADD CONSTRAINT fk_characterisric_chain_key FOREIGN KEY (chain_id) REFERENCES chain_key(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY characteristic_type
    ADD CONSTRAINT fk_characteristic_group FOREIGN KEY (characteristic_group_id) REFERENCES characteristic_group(id) ON UPDATE CASCADE;

ALTER TABLE ONLY characteristic
    ADD CONSTRAINT fk_characteristic_link FOREIGN KEY (link_id) REFERENCES link(id) ON UPDATE CASCADE;

ALTER TABLE ONLY characteristic
    ADD CONSTRAINT fk_characteristic_type FOREIGN KEY (characteristic_type_id) REFERENCES characteristic_type(id) ON UPDATE CASCADE;

ALTER TABLE ONLY congeneric_characteristic
    ADD CONSTRAINT fk_congeneric_characteristic_chain_key FOREIGN KEY (chain_id) REFERENCES chain_key(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY congeneric_characteristic
    ADD CONSTRAINT fk_congeneric_characteristic_element_key FOREIGN KEY (element_id) REFERENCES element_key(id) ON UPDATE CASCADE;

ALTER TABLE ONLY congeneric_characteristic
    ADD CONSTRAINT fk_congeneric_characteristic_link FOREIGN KEY (link_id) REFERENCES link(id) ON UPDATE CASCADE;

ALTER TABLE ONLY congeneric_characteristic
    ADD CONSTRAINT fk_congeneric_characteristic_type FOREIGN KEY (characteristic_type_id) REFERENCES characteristic_type(id) ON UPDATE CASCADE;

ALTER TABLE ONLY dna_chain
    ADD CONSTRAINT fk_dna_chain_chain_key FOREIGN KEY (id) REFERENCES chain_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY dna_chain
    ADD CONSTRAINT fk_dna_chain_matter FOREIGN KEY (matter_id) REFERENCES matter(id) ON UPDATE CASCADE;

ALTER TABLE ONLY dna_chain
    ADD CONSTRAINT fk_dna_chain_notation FOREIGN KEY (notation_id) REFERENCES notation(id) ON UPDATE CASCADE;

ALTER TABLE ONLY dna_chain
    ADD CONSTRAINT fk_dna_chain_piece_type FOREIGN KEY (piece_type_id) REFERENCES piece_type(id) ON UPDATE CASCADE;

ALTER TABLE ONLY dna_chain
    ADD CONSTRAINT fk_dna_chain_product FOREIGN KEY (product_id) REFERENCES product(id) ON UPDATE CASCADE;

ALTER TABLE ONLY dna_chain
    ADD CONSTRAINT fk_dna_chain_remote_db FOREIGN KEY (remote_db_id) REFERENCES remote_db(id) ON UPDATE CASCADE;

ALTER TABLE ONLY element
    ADD CONSTRAINT fk_element_element_key FOREIGN KEY (id) REFERENCES element_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY element
    ADD CONSTRAINT fk_element_notation FOREIGN KEY (notation_id) REFERENCES notation(id) ON UPDATE CASCADE;

ALTER TABLE ONLY fmotiv
    ADD CONSTRAINT fk_fmotiv_chain_key FOREIGN KEY (id) REFERENCES chain_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY fmotiv
    ADD CONSTRAINT fk_fmotiv_element_key FOREIGN KEY (id) REFERENCES element_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY fmotiv
    ADD CONSTRAINT fk_fmotiv_fmotiv_type FOREIGN KEY (fmotiv_type_id) REFERENCES fmotiv_type(id) ON UPDATE CASCADE;

ALTER TABLE ONLY fmotiv
    ADD CONSTRAINT fk_fmotiv_matter FOREIGN KEY (matter_id) REFERENCES matter(id) ON UPDATE CASCADE;

ALTER TABLE ONLY fmotiv
    ADD CONSTRAINT fk_fmotiv_notation FOREIGN KEY (notation_id) REFERENCES notation(id) ON UPDATE CASCADE;

ALTER TABLE ONLY fmotiv
    ADD CONSTRAINT fk_fmotiv_piece_type FOREIGN KEY (piece_type_id) REFERENCES piece_type(id) ON UPDATE CASCADE;

ALTER TABLE ONLY fmotiv
    ADD CONSTRAINT fk_fmotiv_remote_db FOREIGN KEY (remote_db_id) REFERENCES remote_db(id) ON UPDATE CASCADE;

ALTER TABLE ONLY gene
    ADD CONSTRAINT fk_gene_chain_chain_key FOREIGN KEY (id) REFERENCES chain_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY gene
    ADD CONSTRAINT fk_gene_dna_chain FOREIGN KEY (chain_id) REFERENCES dna_chain(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY gene
    ADD CONSTRAINT fk_gene_piece_type FOREIGN KEY (piece_type_id) REFERENCES piece_type(id) ON UPDATE CASCADE;

ALTER TABLE ONLY gene
    ADD CONSTRAINT fk_gene_product FOREIGN KEY (product_id) REFERENCES product(id) ON UPDATE CASCADE;

ALTER TABLE ONLY literature_chain
    ADD CONSTRAINT fk_litarure_chain_translator FOREIGN KEY (translator_id) REFERENCES translator(id);

ALTER TABLE ONLY literature_chain
    ADD CONSTRAINT fk_literature_chain_chain_key FOREIGN KEY (id) REFERENCES chain_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY literature_chain
    ADD CONSTRAINT fk_literature_chain_language FOREIGN KEY (language_id) REFERENCES language(id) ON UPDATE CASCADE;

ALTER TABLE ONLY literature_chain
    ADD CONSTRAINT fk_literature_chain_matter FOREIGN KEY (matter_id) REFERENCES matter(id) ON UPDATE CASCADE;

ALTER TABLE ONLY literature_chain
    ADD CONSTRAINT fk_literature_chain_notation FOREIGN KEY (notation_id) REFERENCES notation(id) ON UPDATE CASCADE;

ALTER TABLE ONLY literature_chain
    ADD CONSTRAINT fk_literature_chain_piece_type FOREIGN KEY (piece_type_id) REFERENCES piece_type(id) ON UPDATE CASCADE;

ALTER TABLE ONLY literature_chain
    ADD CONSTRAINT fk_literature_chain_remote_db FOREIGN KEY (remote_db_id) REFERENCES remote_db(id) ON UPDATE CASCADE;

ALTER TABLE ONLY matter
    ADD CONSTRAINT fk_matter_nature FOREIGN KEY (nature_id) REFERENCES nature(id) ON UPDATE CASCADE;

ALTER TABLE ONLY measure
    ADD CONSTRAINT fk_measure_chain_key FOREIGN KEY (id) REFERENCES chain_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY measure
    ADD CONSTRAINT fk_measure_element_key FOREIGN KEY (id) REFERENCES element_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY measure
    ADD CONSTRAINT fk_measure_matter FOREIGN KEY (matter_id) REFERENCES matter(id) ON UPDATE CASCADE;

ALTER TABLE ONLY measure
    ADD CONSTRAINT fk_measure_notation FOREIGN KEY (notation_id) REFERENCES notation(id) ON UPDATE CASCADE;

ALTER TABLE ONLY measure
    ADD CONSTRAINT fk_measure_piece_type FOREIGN KEY (piece_type_id) REFERENCES piece_type(id) ON UPDATE CASCADE;

ALTER TABLE ONLY measure
    ADD CONSTRAINT fk_measure_remote_db FOREIGN KEY (remote_db_id) REFERENCES remote_db(id) ON UPDATE CASCADE;

ALTER TABLE ONLY music_chain
    ADD CONSTRAINT fk_music_chain_chain_key FOREIGN KEY (id) REFERENCES chain_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY music_chain
    ADD CONSTRAINT fk_music_chain_matter FOREIGN KEY (matter_id) REFERENCES matter(id) ON UPDATE CASCADE;

ALTER TABLE ONLY music_chain
    ADD CONSTRAINT fk_music_chain_notation FOREIGN KEY (notation_id) REFERENCES notation(id) ON UPDATE CASCADE;

ALTER TABLE ONLY music_chain
    ADD CONSTRAINT fk_music_chain_piece_type FOREIGN KEY (piece_type_id) REFERENCES piece_type(id) ON UPDATE CASCADE;

ALTER TABLE ONLY music_chain
    ADD CONSTRAINT fk_music_chain_remote_db FOREIGN KEY (remote_db_id) REFERENCES remote_db(id) ON UPDATE CASCADE;

ALTER TABLE ONLY notation
    ADD CONSTRAINT fk_notation_nature FOREIGN KEY (nature_id) REFERENCES nature(id) ON UPDATE CASCADE;

ALTER TABLE ONLY note
    ADD CONSTRAINT fk_note_element_key FOREIGN KEY (id) REFERENCES element_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY note
    ADD CONSTRAINT fk_note_notation FOREIGN KEY (notation_id) REFERENCES notation(id) ON UPDATE CASCADE;

ALTER TABLE ONLY note
    ADD CONSTRAINT fk_note_tie FOREIGN KEY (tie_id) REFERENCES tie(id) ON UPDATE CASCADE;

ALTER TABLE ONLY piece
    ADD CONSTRAINT fk_piece_gene FOREIGN KEY (gene_id) REFERENCES gene(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY piece_type
    ADD CONSTRAINT fk_piece_type_nature FOREIGN KEY (nature_id) REFERENCES nature(id) ON UPDATE CASCADE;

ALTER TABLE ONLY pitch
    ADD CONSTRAINT fk_pitch_accidental FOREIGN KEY (accidental_id) REFERENCES accidental(id) ON UPDATE CASCADE;

ALTER TABLE ONLY pitch
    ADD CONSTRAINT fk_pitch_instrument FOREIGN KEY (instrument_id) REFERENCES instrument(id) ON UPDATE CASCADE;

ALTER TABLE ONLY pitch
    ADD CONSTRAINT fk_pitch_note FOREIGN KEY (note_id) REFERENCES note(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY pitch
    ADD CONSTRAINT fk_pitch_note_symbol FOREIGN KEY (note_symbol_id) REFERENCES note_symbol(id) ON UPDATE CASCADE;

ALTER TABLE ONLY product
    ADD CONSTRAINT fk_product_piece_type FOREIGN KEY (piece_type_id) REFERENCES piece_type(id) ON UPDATE CASCADE;

ALTER TABLE ONLY remote_db
    ADD CONSTRAINT fk_remote_db_nature FOREIGN KEY (nature_id) REFERENCES nature(id);

INSERT INTO accidental (id, name, description) VALUES (1, '-2', 'Дубль-бемоль');
INSERT INTO accidental (id, name, description) VALUES (2, '-1', 'Бемоль');
INSERT INTO accidental (id, name, description) VALUES (3, '0', 'Бекар');
INSERT INTO accidental (id, name, description) VALUES (4, '1', 'Диез');
INSERT INTO accidental (id, name, description) VALUES (5, '2', 'Дубль-диез');

SELECT pg_catalog.setval('accidental_id_seq', 23, true);

SELECT pg_catalog.setval('characteristic_group_id_seq', 1, false);

INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (4, 'Количество элементов', NULL, NULL, 'Count', false, false, true, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (5, 'Длина обрезания по Садовскому', NULL, NULL, 'CutLength', false, true, false, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (6, 'Энтропия словаря по Садовскому', NULL, NULL, 'CutLengthVocabularyEntropy', false, true, false, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (12, 'Длина', 'Количество позиций для элементов в цепочке', NULL, 'Length', false, true, true, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (15, 'Число возможных цепочек', NULL, NULL, 'PhantomMessagesCount', false, true, false, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (16, 'Частота', 'Или вероятность', NULL, 'Probability', false, false, true, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (26, 'Алфавитная удалённость', 'Вычисляет удалённость с логарифмом основание которого равно мощности алфавита полной цепи', NULL, 'AlphabeticAverageRemoteness', true, true, false, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (27, 'Алфавитная глубина', 'Вычисляет глубину c основание логарифма равным мощности алфавита полной цепи', NULL, 'AlphabeticDepth', true, true, false, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (8, 'Глубина', NULL, NULL, 'Depth', true, true, true, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (10, 'Количество идентифицирующих информаций', NULL, NULL, 'IdentificationInformation', true, true, true, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (11, 'Количество интервалов', NULL, NULL, 'IntervalsCount', true, true, true, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (18, 'Объём цепи', NULL, NULL, 'Volume', true, true, true, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (14, 'Периодичность', NULL, NULL, 'Periodicity', true, true, true, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (17, 'Регулярность', NULL, NULL, 'Regularity', true, true, true, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (3, 'Средняя удалённость', NULL, NULL, 'AverageRemoteness', true, true, true, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (9, 'Среднегеометрический интервал', NULL, NULL, 'GeometricMean', true, true, true, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (2, 'Среднее арифметическое', NULL, NULL, 'ArithmeticMean', true, true, true, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (7, 'Число описательных информаций', NULL, NULL, 'DescriptiveInformation', true, true, true, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (13, 'Нормализованная глубина', 'Глубина, приходящаяся на один элемент цепочки', NULL, 'NormalizedDepth', true, true, true, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (25, 'Сумма интервалов', 'Суммарная длина интервалов данной цепи с учётом привязки', NULL, 'IntervalsSum', true, true, true, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (19, 'Бинарная среднегеометрическая удалённость', 'Среднегеометрическая удалённость между парой элементов', NULL, 'BinaryGeometricMean', true, false, false, true);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (24, 'Нормализованный коэффициент частичной зависимости', NULL, NULL, 'NormalizedPartialDependenceCoefficient', true, false, false, true);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (21, 'Коэффициент частичной зависимости', NULL, NULL, 'PartialDependenceCoefficient', true, false, false, true);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (22, 'Коэффициент взвешенной частичной зависимости ', 'Степень зависимости одной цепи от другой, с учетом «полноты её участия» в составе обеих однородных цепей', NULL, 'InvolvedPartialDependenceCoefficient', true, false, false, true);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (20, 'Избыточность', 'Избыточность кодировки второго элемента относительно себя по сравнению с кодированием относительно первого элемента', NULL, 'Redundancy', true, false, false, true);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (23, 'Коэффициент взаимной зависимости', NULL, NULL, 'MutualDependenceCoefficient', true, false, false, true);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (1, 'Мощность алфавита', NULL, NULL, 'AlphabetCardinality', false, true, false, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (29, 'СКО удалённости', 'Разброс удалённости однородных последовательностей относительно среднего значения', NULL, 'RemotenessStandardDeviation', true, true, false, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (30, 'Ассиметрия удалённости', 'Ассиметрия удалённостей однородных последовательностей относительно среднего значения', NULL, 'RemotenessAsymmetry', true, true, false, false);
INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (28, 'Дисперсия удалённости', 'Разброс удалённости однородных последовательностей относительно среднего значения', NULL, 'RemotenessDispersion', true, true, false, false);

SELECT pg_catalog.setval('characteristic_type_id_seq', 30, true);

SELECT pg_catalog.setval('fmotiv_type_id_seq', 1, false);

SELECT pg_catalog.setval('instrument_id_seq', 1, false);

INSERT INTO language (id, name, description) VALUES (1, 'Русский', 'Русский язык');
INSERT INTO language (id, name, description) VALUES (2, 'Английский', 'Английский язык');
INSERT INTO language (id, name, description) VALUES (3, 'Немецкий', 'Немецкий язык');

SELECT pg_catalog.setval('language_id_seq', 3, true);

INSERT INTO link (id, name, description) VALUES (1, 'К началу', 'Первый интервал считается от начала цепочки, последний - до последнего элемента');
INSERT INTO link (id, name, description) VALUES (2, 'К концу', 'Первый интервал считается от первого элемента, последний - до конца цепочки');
INSERT INTO link (id, name, description) VALUES (3, 'И к началу и к концу', 'Первый интервал считается от начала цепочки, последний до конца цепочки');
INSERT INTO link (id, name, description) VALUES (4, 'Циклическая', 'Первый и последний интервалы суммируются и считаются за один');
INSERT INTO link (id, name, description) VALUES (5, 'Отсутствует', 'Первый и последний интервалы не учитываются');

SELECT pg_catalog.setval('link_id_seq', 5, false);

INSERT INTO nature (id, name, description) VALUES (1, 'Генетические тексты', NULL);
INSERT INTO nature (id, name, description) VALUES (2, 'Музыкальные тексты', NULL);
INSERT INTO nature (id, name, description) VALUES (3, 'Литературные произведения', NULL);

SELECT pg_catalog.setval('nature_id_seq', 3, true);

INSERT INTO notation (id, name, description, nature_id) VALUES (1, 'Нуклеотиды', NULL, 1);
INSERT INTO notation (id, name, description, nature_id) VALUES (2, 'Триплеты', NULL, 1);
INSERT INTO notation (id, name, description, nature_id) VALUES (3, 'Аминокислоты', NULL, 1);
INSERT INTO notation (id, name, description, nature_id) VALUES (4, 'Сегментированая нуклеотидная цепь', NULL, 1);
INSERT INTO notation (id, name, description, nature_id) VALUES (5, 'Слова', 'Буквы, объединённые в последовательности естественным образом', 3);
INSERT INTO notation (id, name, description, nature_id) VALUES (6, 'Ф-мотивы', 'Слова музыкальных произведений, естественные склейки нот', 2);
INSERT INTO notation (id, name, description, nature_id) VALUES (7, 'Такты', 'Группы нот в музыкальных произведениях', 2);
INSERT INTO notation (id, name, description, nature_id) VALUES (8, 'Ноты', 'Элементарные единицы музыкальных произведений', 2);
INSERT INTO notation (id, name, description, nature_id) VALUES (9, 'Буквы', 'Текст не разбитый на слова', 3);

SELECT pg_catalog.setval('notation_id_seq', 9, true);

INSERT INTO note_symbol (id, name, description) VALUES (1, 'A', 'Ля');
INSERT INTO note_symbol (id, name, description) VALUES (2, 'B', 'Си');
INSERT INTO note_symbol (id, name, description) VALUES (3, 'C', 'До');
INSERT INTO note_symbol (id, name, description) VALUES (4, 'D', 'Ре');
INSERT INTO note_symbol (id, name, description) VALUES (5, 'E', 'Ми');
INSERT INTO note_symbol (id, name, description) VALUES (6, 'F', 'Фа');
INSERT INTO note_symbol (id, name, description) VALUES (7, 'G', 'Соль');

SELECT pg_catalog.setval('note_symbol_id_seq', 7, true);

INSERT INTO piece_type (id, name, description, nature_id) VALUES (1, 'Полный геном', 'Вся цепочка без потерянных фрагментов', 1);
INSERT INTO piece_type (id, name, description, nature_id) VALUES (2, 'Полный текст', 'Вся цепочка без потерянных фрагментов', 3);
INSERT INTO piece_type (id, name, description, nature_id) VALUES (3, 'Всё произведение', 'Вся цепочка без потерянных фрагментов', 2);
INSERT INTO piece_type (id, name, description, nature_id) VALUES (4, 'Кодирующая последовательность', 'CDS - coding DNA sequence', 1);
INSERT INTO piece_type (id, name, description, nature_id) VALUES (6, 'Транспортная РНК', 'tRNA - transfer RNA', 1);
INSERT INTO piece_type (id, name, description, nature_id) VALUES (7, 'Некодирущая РНК', 'ncRNA - non-coding RNA', 1);
INSERT INTO piece_type (id, name, description, nature_id) VALUES (8, 'Транспортно-матричная РНК', 'tmRNA - Transfer-messenger RNA', 1);
INSERT INTO piece_type (id, name, description, nature_id) VALUES (9, 'Псевдоген', 'Pseudo', 1);
INSERT INTO piece_type (id, name, description, nature_id) VALUES (10, 'Плазмид', 'Plasmid', 1);
INSERT INTO piece_type (id, name, description, nature_id) VALUES (11, 'Митохондриальный геном', 'Mitochondrion genome', 1);
INSERT INTO piece_type (id, name, description, nature_id) VALUES (12, 'Митохондриальная рРНК', 'Mitochondrion ribosomal RNA', 1);
INSERT INTO piece_type (id, name, description, nature_id) VALUES (5, 'Рибосомальная РНК', 'rRNA - ribosomal RNA', 1);
INSERT INTO piece_type (id, name, description, nature_id) VALUES (13, 'Повторяющийся фрагмент', 'Repeat region', 1);
INSERT INTO piece_type (id, name, description, nature_id) VALUES (14, 'Некодирующая последовательность', 'Non-coding sequence', 1);
INSERT INTO piece_type (id, name, description, nature_id) VALUES (15, 'Геном хлоропласта', 'Chloroplast genome', 1);

SELECT pg_catalog.setval('piece_type_id_seq', 15, true);

INSERT INTO product (id, name, description, piece_type_id) VALUES (2, 'Mitochondrion 16S ribosomal RNA', 'Митохондриальная 16S рибосомальная РНК', 12);
INSERT INTO product (id, name, description, piece_type_id) VALUES (4, 'chromosomal replication initiator protein DnaA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (5, 'hypothetical protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (6, 'peptidase S9 prolyl oligopeptidase active sitedomain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (7, 'DNA ligase, NAD-dependent', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (8, 'VanZ family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (9, 'tRNA-Pro', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (10, 'Endonuclease/exonuclease/phosphatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (11, 'ATP-dependent helicase HrpA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (12, 'protein of unknown function DUF477', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (13, 'LemA family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (14, 'putative signal-transduction protein with CBSdomains', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (15, 'dienelactone hydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (16, 'ABC transporter related', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (17, 'transcription activator effector binding', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (18, 'permease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (19, 'cytidyltransferase-related domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (20, 'outer membrane efflux protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (21, 'Inosine/uridine-preferring nucleoside hydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (22, 'tRNA-Leu', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (23, 'cytochrome c class I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (24, 'beta-lactamase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (25, 'Mannan endo-1,4-beta-mannosidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (26, 'peptidyl-prolyl cis-trans isomerase cyclophilintype', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (27, '', NULL, 9);
INSERT INTO product (id, name, description, piece_type_id) VALUES (28, 'protein of unknown function DUF72', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (29, 'protein of unknown function DUF892', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (30, 'multi-sensor signal transduction histidinekinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (31, 'response regulator receiver protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (32, 'conserved hypothetical protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (33, 'putative two component, sigma54 specific,transcriptional regulator, Fis family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (34, 'short-chain dehydrogenase/reductase SDR', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (35, 'two component transcriptional regulator, LuxRfamily', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (36, 'Ku domainn containing protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (37, 'protein of unknown function DUF558', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (38, 'putative D-ribose-binding periplasmic protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (39, '3-oxoacyl-(acyl-carrier-protein) synthase III', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (40, 'biotin/lipoyl attachment domain-containingprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (41, '2-dehydro-3-deoxyphosphooctonate aldolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (42, 'CTP synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (43, 'PfkB domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (44, 'ABC-2 type transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (45, 'transcriptional regulator, AsnC family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (46, 'Pyrrolo-quinoline quinone', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (47, 'transport-associated', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (48, 'Alcohol dehydrogenase GroES domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (49, 'cyclase/dehydrase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (50, 'two component transcriptional regulator, LytTRfamily', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (51, 'signal transduction histidine kinase, LytS', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (52, 'ROK family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (53, 'peptidylprolyl isomerase FKBP-type', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (54, 'protein of unknown function DUF214', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (55, 'lipolytic protein G-D-S-L family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (56, 'adenylate/guanylate cyclase with integralmembrane sensor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (57, 'RNA polymerase, sigma-24 subunit, ECF subfamily', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (58, 'autotransporter-associated beta strand repeatprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (59, 'gamma-glutamyl phosphate reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (60, 'biotin--acetyl-CoA-carboxylase ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (61, 'multi-sensor hybrid histidine kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (62, 'sulfatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (63, 'protein of unknown function DUF1080', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (64, 'peptide deformylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (65, 'ATP--cobalamin adenosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (66, 'PhoH family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (67, 'acid phosphatase (Class B)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (68, 'DNA topoisomerase (ATP-hydrolyzing)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (69, 'peptidase S41', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (70, 'DNA topoisomerase type IIA subunit B region 2domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (71, 'putative esterase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (72, 'protein of unknown function DUF1684', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (73, 'protein of unknown function DUF1328', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (74, 'glutaredoxin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (75, 'oxidoreductase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (76, 'Xylose isomerase domain protein TIM barrel', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (77, 'putative acetyl xylan esterase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (78, 'Glyoxalase/bleomycin resistanceprotein/dioxygenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (79, 'glycoside hydrolase family 5', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (80, 'methyl-accepting chemotaxis sensory transducer', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (81, 'integral membrane protein-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (82, 'Rieske (2Fe-2S) domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (83, 'sigma 54 modulation protein/ribosomal proteinS30EA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (84, 'ATP-dependent metalloprotease FtsH', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (85, 'Mammalian cell entry related domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (86, 'Paraquat-inducible protein A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (87, 'small GTP-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (88, 'Redoxin domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (89, 'GCN5-related N-acetyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (90, 'phosphohistidine phosphatase, SixA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (91, 'Oar protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (92, 'OsmC family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (93, 'DTW domain containing protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (94, 'phosphofructokinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (95, 'putative sodium-dependent transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (96, 'SCP-like extracellular', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (97, 'aldo/keto reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (98, 'transcriptional regulator, AraC family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (99, 'amino acid-binding ACT domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (100, 'acetolactate synthase, large subunit,biosynthetic type', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (101, 'cyclic nucleotide-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (102, 'histidine kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (103, 'periplasmic binding protein/LacI transcriptionalregulator', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (104, 'putative alpha-glucosidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (105, 'NAD(P)H quinone oxidoreductase, PIG3 family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (106, 'NAD-dependent epimerase/dehydratase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (107, 'queuine tRNA-ribosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (108, 'glutamine amidotransferase of anthranilatesynthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (109, 'ATP-cone domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (110, 'histidine triad (HIT) protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (111, 'DNA protecting protein DprA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (112, 'carbohydrate kinase, YjeF related protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (113, 'holo-acyl-carrier-protein synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (114, 'pyridoxal phosphate biosynthetic protein PdxJ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (115, 'citrate synthase I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (116, 'imidazole glycerol phosphate synthase, glutamineamidotransferase subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (117, 'Imidazoleglycerol-phosphate dehydratase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (118, 'histidinol-phosphate aminotransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (119, 'Histidinol dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (120, 'GMP synthase, large subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (121, 'major facilitator superfamily MFS_1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (122, 'phosphoesterase PA-phosphatase related', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (123, 'ribosomal protein L17', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (124, 'DNA-directed RNA polymerase, alpha subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (125, 'ribosomal protein S4', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (126, 'ribosomal protein S11', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (127, 'ribosomal protein S13', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (128, 'methionine aminopeptidase, type I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (129, 'preprotein translocase, SecY subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (130, 'ribosomal protein L15', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (131, 'ribosomal protein S5', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (132, 'ribosomal protein L18P/L5E', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (133, 'ribosomal protein L6', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (134, 'ribosomal protein S8', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (135, 'ribosomal protein S14', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (136, 'ribosomal protein L5', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (137, 'ribosomal protein L24', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (138, 'ribosomal protein L14', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (139, 'ribosomal protein S17', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (140, 'ribosomal protein L29', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (141, 'ribosomal protein L16', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (142, 'ribosomal protein S3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (143, 'ribosomal protein L22', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (144, 'ribosomal protein S19', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (145, 'ribosomal protein L2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (146, 'Ribosomal protein L25/L23', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (147, 'ribosomal protein L4/L1e', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (148, 'ribosomal protein L3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (149, 'ribosomal protein S10', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (150, 'translation elongation factor G', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (151, 'ribosomal protein S7', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (152, 'ribosomal protein S12', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (153, 'DNA-directed RNA polymerase, beta'' subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (154, 'DNA-directed RNA polymerase, beta subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (155, 'ribosomal protein L7/L12', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (156, 'ribosomal protein L10', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (157, 'ribosomal protein L1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (158, 'ribosomal protein L11', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (159, 'NusG antitermination factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (160, 'preprotein translocase, SecE subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (161, 'tRNA-Trp', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (162, 'translation elongation factor Tu', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (163, 'tRNA-Thr', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (164, 'MotA/TolQ/ExbB proton channel', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (165, 'Tetratricopeptide TPR_4', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (166, 'peptidase M48 Ste24p', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (167, 'Fe-S metabolism associated SufE', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (168, 'TatD-related deoxyribonuclease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (169, 'UBA/THIF-type NAD/FAD binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (170, 'Antibiotic biosynthesis monooxygenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (171, 'iron-containing alcohol dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (172, 'nicotinamide-nucleotide adenylyltransferase,putative', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (173, 'nicotinamide mononucleotide transporter PnuC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (174, 'transglutaminase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (175, 'integral membrane sensor signal transductionhistidine kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (176, 'protein-L-isoaspartate O-methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (177, 'glycosyl transferase family 2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (178, 'peptide methionine sulfoxide reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (179, 'Appr-1-p processing domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (180, 'glycoside hydrolase family 3 domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (181, 'TPR repeat-containing protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (182, 'protein of unknown function UPF0153', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (183, 'protein of unknown function DUF748', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (184, 'thioesterase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (185, 'translation initiation factor IF-2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (186, 'EPSP synthase (3-phosphoshikimate1-carboxyvinyltransferase)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (187, 'Holliday junction DNA helicase RuvB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (188, 'Exonuclease RNase T and DNA polymerase III', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (189, 'histidinol-phosphate phosphatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (190, '7-cyano-7-deazaguanine reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (191, 'RNA binding metal dependent phosphohydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (192, 'eight transmembrane protein EpsH', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (193, 'Peptidoglycan-binding LysM', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (194, 'Patatin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (195, 'glycosyl transferase group 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (196, 'polysaccharide biosynthesis protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (197, 'O-antigen polymerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (198, 'cell cycle protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (199, 'ribonuclease, Rne/Rng family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (200, 'YHS domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (201, 'Methyltransferase type 11', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (202, 'protein of unknown function DUF1223', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (203, 'phosphate transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (204, 'Radical SAM domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (205, 'protein of unknown function DUF547', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (206, 'SNARE associated Golgi protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (207, 'pyridine nucleotide-disulphide oxidoreductasedimerisation region', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (208, 'conserved repeat domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (209, 'Immunoglobulin I-set domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (210, 'membrane protein-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (211, 'glycosyl hydrolase family 88', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (212, 'two component transcriptional regulator, wingedhelix family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (213, 'efflux transporter, RND family, MFP subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (214, 'von Willebrand factor type A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (215, 'Alpha-N-arabinofuranosidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (216, 'putative lipoprotein signal peptide', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (217, 'Inositol-phosphate phosphatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (218, 'transcriptional regulator, LysR family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (219, 'isochorismatase hydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (220, 'GreA/GreB family elongation factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (221, 'alpha amylase all-beta', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (222, 'cytochrome c peroxidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (223, 'protein of unknown function DUF437', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (224, 'transcriptional regulator, XRE family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (225, 'bifunctional deaminase-reductase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (226, 'serine/threonine protein kinase with WD40repeats', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (227, 'glycoside hydrolase family 43', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (228, 'CheW protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (229, 'integral membrane sensor hybrid histidinekinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (230, 'Silent information regulator protein Sir2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (231, 'radical SAM enzyme, Cfr family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (232, 'Tryptophanase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (233, 'Integrin alpha beta-propellor repeat protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (234, 'PAS/PAC sensor signal transduction histidinekinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (235, 'pectate lyase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (236, 'oxidoreductase molybdopterin binding', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (237, 'sodium:dicarboxylate symporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (238, 'TonB family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (239, 'flagellin domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (240, 'carbon storage regulator, CsrA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (241, 'protein of unknown function DUF180', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (242, 'flagellar hook-associated protein 3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (243, 'flagellar hook-associated protein FlgK', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (244, 'flagellar P-ring protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (245, 'flagellar L-ring protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (246, 'Flagellar basal body P-ring biosynthesisprotein-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (247, 'flagellar basal-body rod protein FlgG', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (248, 'protein of unknown function DUF1078', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (249, 'OmpA/MotB domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (250, 'RNA polymerase, sigma 28 subunit, FliA/WhiG', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (251, 'Flagellar GTP-binding protein-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (252, 'flagellar biosynthesis protein FlhA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (253, 'type III secretion exporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (254, 'type III secretion system inner membrane Rprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (255, 'export protein FliQ family 3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (256, 'flagellar biosynthetic protein FliP', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (257, 'flagellar motor switch protein FliN', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (258, 'flagellar motor switch protein FliM', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (259, 'flagellar basal body-associated protein FliL', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (260, 'flagellar hook capping protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (261, 'flagellar protein export ATPase FliI', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (262, 'flagellar motor switch protein FliG', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (263, 'flagellar M-ring protein FliF', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (264, 'flagellar hook-basal body complex subunit FliE', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (265, 'flagellar basal-body rod protein FlgC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (266, 'flagellar basal-body rod protein FlgB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (267, 'MCP methyltransferase, CheR-type', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (268, 'response regulator receiver modulated CheBmethylesterase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (269, 'CheD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (270, 'putative signal transduction protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (271, 'CheA signal transduction histidine kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (272, 'two component, sigma54 specific, transcriptionalregulator, Fis family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (273, 'flagellar protein FliS', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (274, 'flagellar hook-associated 2 domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (275, 'phosphoheptose isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (276, 'hydrolase, HAD-superfamily, subfamily IIIA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (277, 'methyltransferase FkbM family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (278, '2-C-methyl-D-erythritol 4-phosphatecytidylyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (279, 'Tetratricopeptide TPR_2 repeat protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (280, 'PAS/PAC sensor hybrid histidine kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (281, 'ComEC/Rec2-related protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (282, 'proton-translocating NADH-quinoneoxidoreductase, chain N', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (283, 'proton-translocating NADH-quinoneoxidoreductase, chain M', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (284, 'proton-translocating NADH-quinoneoxidoreductase, chain L', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (285, 'NADH-ubiquinone oxidoreductase chain 4L', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (286, 'NADH-ubiquinone/plastoquinone oxidoreductasechain 6', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (287, 'NADH-quinone oxidoreductase, chain I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (288, 'NADH dehydrogenase (quinone)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (289, 'ferredoxin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (290, 'NADH-quinone oxidoreductase, E subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (291, 'NADH dehydrogenase I, D subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (292, 'NADH (or F420H2) dehydrogenase, subunit C', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (293, 'NADH-quinone oxidoreductase, B subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (294, 'Acetyl xylan esterase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (295, 'protein of unknown function DUF1568', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (296, 'ASPIC/UnbV domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (297, 'FG-GAP repeat protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (298, 'pseudouridine synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (299, 'GH3 auxin-responsive promoter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (300, 'alanine racemase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (301, 'shikimate 5-dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (302, 'peptidase A24A domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (303, 'galactokinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (304, 'histone family protein DNA-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (305, 'folate-binding protein YgfZ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (306, 'phospho-2-dehydro-3-deoxyheptonate aldolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (307, 'acriflavin resistance protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (308, 'transcriptional regulator, TetR family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (309, 'malate dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (310, 'transcriptional regulator, LacI family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (311, 'TonB-dependent receptor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (312, 'Carbamoyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (313, 'Integrase catalytic region', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (314, 'restriction endonuclease-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (315, 'sucrose-6F-phosphate phosphohydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (316, 'putative signal transduction protein with Nachtdomain', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (317, 'beta-lactamase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (318, 'cardiolipin synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (319, 'peptidase M50', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (320, 'lipoprotein, putative', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (321, 'WD40 domain protein beta Propeller', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (322, 'Phosphoglycerate mutase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (323, 'Kelch repeat-containing protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (324, 'YceI family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (325, 'Polyprenyl synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (326, 'cytochrome C oxidase mono-heme subunit/FixO', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (327, 'Cobyrinic acid ac-diamide synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (328, 'dipeptidylaminopeptidase/acylaminoacyl-peptidase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (329, 'anti-ECFsigma factor, ChrR', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (330, 'cytoplasmic membrane protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (331, 'N-acetylmuramoyl-L-alanine amidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (332, 'TonB-dependent receptor plug', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (333, 'tRNA-Ser', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (334, 'oxygen-independent coproporphyrinogen IIIoxidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (335, '6-pyruvoyl tetrahydropterin synthase andhypothetical protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (336, 'SUF system FeS assembly protein, NifU family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (337, 'cysteine desulfurase, SufS subfamily', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (338, 'pyruvate flavodoxin/ferredoxin oxidoreductasedomain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (339, 'dihydroorotate dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (340, 'conserved hypothetical secreted protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (341, 'amino acid carrier protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (342, 'aminotransferase class-III', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (343, 'membrane-flanked domain', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (344, 'Ferritin Dps family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (345, 'aminotransferase class I and II', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (346, 'KpsF/GutQ family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (347, 'hydro-lyase, Fe-S type, tartrate/fumaratesubfamily, beta subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (348, 'protein of unknown function UPF0047', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (349, 'fumarate lyase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (350, 'Abortive infection protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (351, 'PKD domain containing protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (352, 'type II and III secretion system protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (353, 'UvrD/REP helicase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (354, 'ATP-dependent nuclease subunit B-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (355, 'ribosomal protein S20', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (356, 'anti-FecI sigma factor, FecR', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (357, 'protein of unknown function DUF164', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (358, 'DNA gyrase, B subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (359, 'DNA gyrase, A subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (360, 'sugar transport family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (361, 'protein of unknown function DUF985', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (362, 'methyltransferase small', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (363, 'diaminopimelate decarboxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (364, 'Water Stress and Hypersensitive response domainprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (365, 'succinate dehydrogenase or fumarate reductase,flavoprotein subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (366, 'succinate dehydrogenase (or fumarate reductase)cytochrome b subunit, b558 family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (367, 'Coenzyme F390 synthetase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (368, 'NUDIX hydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (369, 'formyltetrahydrofolate deformylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (370, 'carbamoyl-phosphate synthase, large subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (371, 'type II secretion system protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (372, 'tRNA-Gly', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (373, 'methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (374, 'protein of unknown function DUF140', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (375, 'Argininosuccinate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (376, 'ornithine carbamoyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (377, 'acetylornithine and succinylornithineaminotransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (378, 'acetylglutamate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (379, 'arginine biosynthesis bifunctional protein ArgJ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (380, 'Orn/DAP/Arg decarboxylase 2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (381, 'Homospermidine synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (382, 'N-acetyl-gamma-glutamyl-phosphate reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (383, 'ribosomal protein S9', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (384, 'ribosomal protein L13', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (385, 'protein of unknown function DUF423', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (386, 'Polynucleotide adenylyltransferase region', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (387, 'transaldolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (388, 'Glycoside hydrolase, family 20, catalytic core', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (389, 'Alanine dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (390, 'phosphoribosylaminoimidazole carboxylase,catalytic subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (391, 'RDD domain containing protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (392, 'Aldehyde Dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (393, 'Phosphoenolpyruvate carboxykinase (GTP)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (394, 'cation diffusion facilitator family transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (395, 'conserved hypothetical protein-putativeacetolactate synthase small regulatory subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (396, 'Glucose--fructose oxidoreductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (397, 'putative methyl-accepting chemotaxis sensorytransducer', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (398, 'RNA-binding S4 domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (399, 'protein of unknown function UPF0044', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (400, 'Pyridoxal-5''-phosphate-dependent protein betasubunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (401, 'ribosome small subunit-dependent GTPase A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (402, 'DoxX family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (403, 'protein of unknown function DUF692', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (404, 'D-isomer specific 2-hydroxyacid dehydrogenaseNAD-binding', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (405, 'putative ferredoxin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (406, '627aa long 2-oxoacid--ferredoxinoxidoreductasealpha subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (407, 'pyruvate ferredoxin/flavodoxin oxidoreductase,beta subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (408, 'NADH-ubiquinone/plastoquinone oxidoreductasechain 3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (409, 'NADH dehydrogenase (ubiquinone) 30 kDa subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (410, '4Fe-4S ferredoxin iron-sulfur binding domainprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (411, 'acyltransferase family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (412, '6-phosphogluconate dehydrogenase,decarboxylating', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (413, 'pyridoxamine 5''-phosphate oxidase-relatedFMN-binding', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (414, 'putative transcriptional regulator, GntR family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (415, 'FAD-dependent pyridine nucleotide-disulphideoxidoreductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (416, 'putative transmembrane protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (417, '3-methyl-2-oxobutanoatehydroxymethyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (418, 'pantoate--beta-alanine ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (419, 'Phosphopantothenoylcysteine decarboxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (420, 'DNA/pantothenate metabolism flavoprotein domainprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (421, 'putative transcriptional acitvator, Baf family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (422, 'quinolinate synthetase complex, A subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (423, 'nicotinate-nucleotide pyrophosphorylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (424, 'L-aspartate oxidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (425, 'Guanylate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (426, 'S23 ribosomal protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (427, 'RNA modification enzyme, MiaB family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (428, 'Lytic transglycosylase catalytic', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (429, 'Rhomboid family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (430, 'globin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (431, 'response regulator receiver sensor signaltransduction histidine kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (432, 'glucose sorbosone dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (433, 'pyridoxal-phosphate dependent TrpB-like enzyme', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (434, 'Glutamate dehydrogenase (NADP(+))', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (435, 'pyruvate phosphate dikinasePEP/pyruvate-binding', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (436, 'Protein of unknown function DUF933', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (437, 'heat shock protein 15', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (438, 'carbohydrate kinase, thermoresistant glucokinasefamily', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (439, 'Penicillin amidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (440, 'uracil phosphoribosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (441, 'ATPase BadF/BadG/BcrA/BcrD type', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (442, 'FAD linked oxidase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (443, 'cytochrome b subunit of formatedehydrogenase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2045, 'Elongation factor Tu', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (444, 'multiple antibiotic resistance (MarC)-relatedprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (445, 'acetylornithine deacetylase orsuccinyl-diaminopimelate desuccinylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (446, 'pyridoxamine 5''-phosphate oxidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (447, 'dTDP-4-dehydrorhamnose reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (448, 'biotin/lipoate A/B protein ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (449, 'RNA methyltransferase, TrmA family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (450, 'tRNA/rRNA methyltransferase (SpoU)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (451, '3-beta hydroxysteroid dehydrogenase/isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (452, 'AMP-dependent synthetase and ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (453, '3''-5'' exonuclease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (454, 'histone deacetylase superfamily', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (455, 'exsB protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (456, 'Exopolysaccharide synthesis ExoD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (457, '4-alpha-glucanotransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (458, 'alpha amylase catalytic region', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (459, 'ferrous iron transport protein B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (460, 'FeoA family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (461, 'ATP synthase F1, epsilon subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (462, 'ATP synthase F1, beta subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (463, 'ATP synthase F1, gamma subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (464, 'ATP synthase F1, alpha subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (465, 'ATP synthase F0, B subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (466, 'H+transporting two-sector ATPase C subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (467, 'ATP synthase F0, A subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (468, 'putative methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (469, 'glucose inhibited division protein A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (470, 'tRNA modification GTPase TrmE', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (471, 'recA protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (472, 'Citrate transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (473, 'Glutamate synthase (ferredoxin)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (474, 'glutamate synthase, NADH/NADPH, small subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (475, 'thiol:disulfide interchange protein DsbD,putative', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (476, 'protein of unknown function DUF59', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (477, 'Cupin 2 conserved barrel domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (478, 'conserved hypothetical membrane spanningprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (479, 'transcriptional regulator, Crp/Fnr family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (480, 'protein of unknown function DUF6 transmembrane', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (481, 'SWIB/MDM2 domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (482, 'magnesium transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (483, 'secretion protein HlyD family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (484, 'MscS Mechanosensitive ion channel', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (485, 'Integral membrane protein TerC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (486, 'heat shock protein Hsp20', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (487, 'phospholipase A1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (488, 'transcriptional regulator, MarR family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (489, 'TIM-barrel protein, nifR3 family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (490, 'LigA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (491, 'RarD protein, DMT superfamily transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (492, 'Uracil-DNA glycosylase superfamily', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (493, 'DEAD/DEAH box helicase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (494, 'RNP-1 like RNA-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (495, 'molybdopterin biosynthesis MoaE protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (496, 'thiamineS protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (497, 'molybdenum cofactor biosynthesis protein C', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (498, 'molybdenum cofactor synthesis domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (499, 'molybdenum cofactor biosynthesis protein A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (500, 'DNA polymerase III, beta subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (501, 'CDP-diacylglycerol--serineO-phosphatidyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (502, 'threonine dehydratase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (503, 'N-acetylmuramyl-L-alanine amidase, negativeregulator of AmpC, AmpD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (504, 'pyruvate carboxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (505, 'peptidase M16 domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (506, 'SMP-30/Gluconolaconase/LRE domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (507, '2-nitropropane dioxygenase, NPD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (508, 'acyl-ACP thioesterase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (509, 'ATPase involved in DNA repair', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (510, 'adenylate/guanylate cyclase with Chase sensor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (511, 'glycoside hydrolase family 2 sugar binding', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (512, 'filamentation induced by cAMP protein Fic', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (513, 'Methyltransferase type 12', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (514, 'response regulator receiver modulated serinephosphatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (515, 'glycyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (516, 'General substrate transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (517, 'Beta-N-acetylhexosaminidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (518, 'glucosamine-6-phosphate isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (519, 'beta-phosphoglucomutase family hydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (520, 'Kojibiose phosphorylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (521, 'parallel beta-helix repeat', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (522, 'Nitrite reductase (NO-forming)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (523, 'translation initiation factor IF-1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (524, 'tRNA-Met', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (525, 'HNH endonuclease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (526, 'DNA binding domain protein, excisionase family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (527, 'AAA ATPase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (528, 'putative phage repressor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (529, 'band 7 protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (530, 'Nicotinamidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (531, 'NAD+ synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (532, 'nicotinate phosphoribosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (533, 'tRNA-Lys', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (534, 'tRNA-Cys', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (535, 'tRNA-Phe', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (536, 'tRNA-OTHER', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (537, 'tRNA-His', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (538, 'tRNA-Val', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (539, 'tRNA-Asn', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (540, 'tRNA-Gln', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (541, 'tRNA-Tyr', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (542, 'tRNA-Ile', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (543, 'putative RNA polymerase, sigma 28 subunit,FliA/WhiG family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (544, 'metallophosphoesterase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (545, 'NADPH-dependent FMN reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (546, 'Tetratricopeptide domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (547, 'FAD dependent oxidoreductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (548, 'transcriptional regulator, GntR family withaminotransferase domain', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (549, 'low molecular weight phosphotyrosine proteinphosphatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (550, 'glutathione S-transferase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (551, 'Stress responsive alpha-beta barrel domainprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (552, 'protein of unknown function DUF1294', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (553, 'topoisomerase DNA binding C4 zinc finger domainprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (554, 'restriction endonuclease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (555, 'type I restriction-modification system, Msubunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (556, 'anticodon nuclease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2046, '50S ribosomal protein L3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (557, 'restriction modification system DNA specificitydomain', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (558, 'type I site-specific deoxyribonuclease, HsdRfamily', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (559, 'Alpha/beta hydrolase fold-3 domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (560, 'alkyl hydroperoxide reductase/ Thiol specificantioxidant/ Mal allergen', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (561, 'Endo-1,4-beta-xylanase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (562, 'Non-specific serine/threonine protein kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (563, 'trehalose synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (564, 'transcriptional regulator, MerR family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (565, 'beta-Ig-H3/fasciclin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (566, 'Beta-lactamase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (567, 'acyltransferase 3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (568, 'protein of unknown function DUF1555', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (569, 'protein of unknown function DUF899 thioredoxinfamily protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (570, 'Fibronectin type III domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (571, 'signal transduction histidine kinase, nitrogenspecific, NtrB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (572, 'PRC-barrel domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (573, 'chaperone DnaJ domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (574, 'protein of unknown function DUF1498', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (575, '6-phosphogluconate dehydrogenase NAD-binding', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (576, 'GAF sensor hybrid histidine kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (577, 'putative DNA topoisomerase I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (578, 'protein of unknown function UPF0126', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (579, 'L-asparaginase, type II', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (580, 'amidohydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (581, 'LmbE family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (582, 'Ig family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (583, 'Sialate O-acetylesterase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (584, 'excinuclease ABC C subunit domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (585, 'competence/damage-inducible protein CinA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (586, 'Arabinogalactan endo-1,4-beta-galactosidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (587, 'protein of unknown function DUF883 ElaB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (588, 'NAD(P)H dehydrogenase (quinone)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (589, 'transcriptional regulator, HxlR family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (590, 'glycoside hydrolase family 28', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (591, 'Alcohol dehydrogenase zinc-binding domainprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (592, 'Entericidin EcnAB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (593, 'cupin 2, conserved barrel', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (594, 'phosphoribosylaminoimidazole-succinocarboxamidesynthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (595, 'tRNA-Glu', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (596, 'conserved hypothetical integral membraneprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (597, 'Adenine-specific DNA methylase containing aZn-ribbon-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (598, 'helicase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (599, 'protein of unknown function DUF218', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (600, 'Endoribonuclease L-PSP', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (601, 'conjugative relaxase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (602, 'ATPase AAA-2 domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (603, 'alpha/beta hydrolase fold', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (604, 'protein of unknown function DUF932', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (605, 'integrase family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (606, 'Heavy metal transport/detoxification protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (607, 'channel protein, hemolysin III family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (608, 'DNA-directed RNA polymerase, omega subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (609, 'chaperonin GroEL', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (610, 'putative circadian clock protein, KaiC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (611, 'glycosidase PH1107-related', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (612, 'membrane protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (613, 'PilT protein domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (614, 'anion transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (615, 'peptidase C10 streptopain', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (616, 'ExsB family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (617, 'ribose-phosphate pyrophosphokinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (618, 'Peptidyl-dipeptidase Dcp', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (619, 'Glucosylceramidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (620, 'sugar (Glycoside-Pentoside-Hexuronide)transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (621, 'glycoside hydrolase family 16', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (622, 'DoxX', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (623, 'TGF-beta receptor type I/II extracellularregion', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (624, 'Formate--tetrahydrofolate ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (625, 'putative PTS IIA-like nitrogen-regulatoryprotein PtsN', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (626, 'alkylhydroperoxidase like protein, AhpD family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (627, 'transcriptional regulator, DeoR family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (628, 'Propanediol utilization protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (629, 'microcompartments protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (630, 'acetate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (631, 'Ethanolamine utilization proteinEutN/carboxysome structural protein Ccml', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (632, 'ethanolamine utilization proteinEutN/carboxysome structural protein CcmL', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (633, 'class II aldolase/adducin family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (634, 'Lactate/malate dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (635, 'carbohydrate kinase FGGY', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (636, 'L-rhamnose isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (637, 'rhamnulose-1-phosphate aldolase/alcoholdehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (638, 'transcriptional regulator, ArsR family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (639, 'Activator of Hsp90 ATPase 1 family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (640, 'protein of unknown function DUF81', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (641, 'protein of unknown function DUF1428', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (642, 'Inner membrane CreD family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (643, 'Extradiol ring-cleavage dioxygenase class IIIprotein subunit B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (644, 'Thymidylate synthase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (645, 'L-amino-acid oxidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (646, 'protein of unknown function DUF303acetylesterase putative', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (647, 'YesW', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (648, 'alpha/beta hydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (649, 'conserved hypothetical protein-transmembraneprediction', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (650, 'Na+/solute symporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (651, 'L-rhamnose 1-epimerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (652, 'glycoside hydrolase family 39', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (653, 'Beta-galactosidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (654, '2-alkenal reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (655, 'tRNA-Arg', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (656, 'Nitrilase/cyanide hydratase and apolipoproteinN-acyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (657, 'L-arabinose isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (658, 'Aldose 1-epimerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (659, 'SSS sodium solute transporter superfamily', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (660, 'serine/threonine protein kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (661, 'Arabinan endo-1,5-alpha-L-arabinosidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (662, 'protein of unknown function DUF162', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (663, 'protein of unknown function DUF224 cysteine-richregion domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (664, 'carbon starvation protein CstA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (665, 'adenylosuccinate lyase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (666, 'protein of unknown function DUF1255', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (667, 'DNA polymerase III, delta subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (668, 'cytochrome B561', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (669, 'glycosyl transferase family 39', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (670, 'DegT/DnrJ/EryC1/StrS aminotransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (671, 'formyl transferase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (672, 'polysaccharide deacetylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (673, 'permease YjgP/YjgQ family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (674, 'Haloacid dehalogenase domain protein hydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (675, 'amidohydrolase 2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (676, 'protein-(glutamine-N5) methyltransferase,ribosomal protein L3-specific', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (677, 'AAA ATPase central domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (678, 'Rhodanese domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (679, 'protein of unknown function DUF302', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (680, '2'',3''-cyclic-nucleotide 2''-phosphodiesterase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (681, 'cytochrome d ubiquinol oxidase, subunit II', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (682, 'cytochrome bd ubiquinol oxidase subunit I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (683, 'thioredoxin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (684, 'protein of unknown function DUF395 YeeE/YedE', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (685, 'Polysulphide reductase NrfD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (686, 'molybdopterin oxidoreductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (687, 'membrane protein involved in aromatichydrocarbon degradation', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (688, 'Methionine biosynthesis MetW protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (689, 'homoserine O-acetyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (690, 'protein of unknown function DUF28', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (691, '60 kDa inner membrane insertion protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (692, 'protein of unknown function DUF37', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (693, 'ribonuclease P protein component', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (694, 'ribosomal protein L34', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (695, 'Lysine exporter protein (LYSE/YGGA)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (696, 'phosphodiesterase, MJ0936 family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (697, 'CDP-alcohol phosphatidyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (698, 'protein of unknown function DUF45', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (699, 'Peptidylprolyl isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (700, 'Auxin Efflux Carrier', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (701, 'glycerol kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (702, 'MATE efflux family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (703, 'protein of unknown function DUF167', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (704, 'Cys/Met metabolism pyridoxal-phosphate-dependentprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (705, 'exodeoxyribonuclease III Xth', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (706, 'CinA domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (707, 'protein of unknown function DUF500', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (708, 'protein of unknown function DUF1025', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (709, 'putative ABC transporter, permease protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (710, 'HI0933 family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (711, 'RNA binding S1 domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (712, 'signal peptidase I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (713, 'deoxyribose-phosphate aldolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (714, 'ABC-type Na+ efflux pump, permease component', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (715, 'Arylamine N-acetyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (716, 'putative transmembrane anti-sigma factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (717, 'hydrogenase, Fe-only', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (718, 'NADH dehydrogenase (ubiquinone) 24 kDa subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (719, 'Inorganic diphosphatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (720, 'heavy metal efflux pump, CzcA family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (721, 'pyruvate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (722, 'UTP--glucose-1-phosphateuridylyltransferase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (723, 'nucleotidyl transferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (724, 'protein of unknown function DUF198', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (725, 'glycosyl transferase, WecB/TagA/CpsF family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (726, 'HAD-superfamily hydrolase, subfamily IA, variant3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (727, 'magnesium and cobalt transport protein CorA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (728, 'ThiJ/PfpI domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (729, 'phenazine biosynthesis protein PhzF family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (730, 'transcriptional regulator, BadM/Rrf2 family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (731, 'major intrinsic protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (732, 'polysaccharide export protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (733, 'capsular exopolysaccharide family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (734, 'Sua5/YciO/YrdC/YwlC family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (735, 'ribosomal protein S18', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (736, 'Excinuclease ABC C subunit domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (737, 'general secretion pathway protein G', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (738, 'ApbE family lipoprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (739, 'putative lipoprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (740, 'type II secretion system protein E', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (741, 'Chorismate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (742, 'pyrimidine-nucleoside phosphorylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (743, 'deoxyhypusine synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (744, 'DNA-formamidopyrimidine glycosylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (745, 'protein of unknown function DUF255', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (746, 'putative adenylate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (747, 'peptidase U32', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (748, 'PDZ/DHR/GLGF domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (749, 'protein of unknown function DUF124', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (750, 'Nucleotide diphosphatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (751, 'Smr protein/MutS2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (752, 'twin-arginine translocation protein, TatA/Efamily subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (753, 'protein of unknown function DUF107', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (754, 'protein of unknown function DUF1432', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (755, 'cell divisionFtsK/SpoIIIE', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (756, 'mannose-6-phosphate isomerase type I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (757, 'GrpE protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (758, 'chaperone protein DnaJ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (759, 'phosphoribosylglycinamide formyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (760, 'ribosomal L11 methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (761, 'acetyl-CoA carboxylase, biotin carboxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (762, 'acetyl-CoA carboxylase, biotin carboxyl carrierprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (763, 'tRNA-Ala', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (764, '23S ribosomal RNA', NULL, 6);
INSERT INTO product (id, name, description, piece_type_id) VALUES (765, '5S ribosomal RNA', NULL, 6);
INSERT INTO product (id, name, description, piece_type_id) VALUES (766, 'preprotein translocase, SecA subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (767, 'apolipoprotein N-acyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (768, 'TspO and MBR like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (769, 'peptidase M20', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (770, 'cell wall surface anchor family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (771, 'Aldehyde dehydrogenase (NAD(+))', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (772, 'protein of unknown function DUF1680', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (773, '3,4-dihydroxy-2-butanone 4-phosphate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (774, '6,7-dimethyl-8-ribityllumazine synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (775, 'NusB antitermination factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (776, 'signal recognition particle-docking proteinFtsY', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (777, 'molybdate ABC transporter, inner membranesubunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2047, '50S ribosomal protein L23', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (778, 'molybdenum ABC transporter, periplasmicmolybdate-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (779, 'WD-40 repeat protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (780, 'GtrA family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (781, 'transcriptional regulator, LuxR family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (782, 'cobalamin synthesis protein P47K', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (783, 'protein of unknown function DUF21', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (784, 'protease Do', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (785, 'heavy metal translocating P-type ATPase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (786, 'nitrate transporter, putative', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (787, 'formate/nitrite transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (788, 'precorrin-3B synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (789, 'FAD-binding domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (790, 'MOSC domain containing protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (791, 'Molybdopterin-guanine dinucleotide biosynthesisprotein A-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (792, 'glycosyltransferase 36', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (793, 'extracellular solute-binding protein family 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (794, 'binding-protein-dependent transport systemsinner membrane component', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (795, 'coagulation factor 5/8 type domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (796, 'glycoside hydrolase family 31', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (797, 'xylose isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (798, 'YbaK/prolyl-tRNA synthetase associated region', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (799, 'nicotinate (nicotinamide) nucleotideadenylyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (800, 'iojap-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (801, 'type IV pilus biogenesis protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (802, 'exopolysaccharide biosynthesis polyprenylglycosylphosphotransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (803, 'exodeoxyribonuclease VII, large subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (804, 'methionine-R-sulfoxide reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (805, 'Glycerate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (806, 'electron transport protein SCO1/SenC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (807, 'blue (type 1) copper domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (808, 'Nitrous-oxide reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (809, 'Carbohydrate-binding and sugar hydrolysis', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (810, 'helix-turn-helix, type 11 domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (811, 'Neprilysin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (812, 'protein of unknown function DUF418', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (813, 'Carbohydrate-binding CenC domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (814, 'conserved hypothetical protein-signal peptideand transmembrane prediction', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (815, 'transcriptional regulator, PadR-like family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (816, 'membrane-bound metal-dependent hydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (817, 'nitroreductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (818, 'adenylate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (819, 'putative transcriptional regulator, PaaX family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (820, 'glycosyl transferase family 28', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (821, 'putative secreted protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (822, 'Carboxypeptidase Taq', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (823, 'Phosphotransferase system, phosphocarrierprotein HPr', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (824, '3-deoxy-D-manno-octulosonatecytidylyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (825, 'carbamoyl-phosphate synthase, small subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (826, 'lysine 2,3-aminomutase YodO family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (827, 'plasmid pRiA4b ORF-3 family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (828, 'phosphatidate cytidylyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (829, 'undecaprenyl diphosphate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (830, 'transcriptional coactivator/pterin dehydratase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (831, 'purine phosphorylase family 2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (832, 'phospholipase D/Transphosphatidylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (833, 'conserved hypothetical protein; predictedATP/GTP-binding', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (834, 'glycine dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (835, 'endonuclease III', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (836, 'Pectate lyase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (837, 'plasmid stabilization system', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (838, 'addiction module component, TIGR02574 family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (839, 'transposase IS3/IS911 family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (840, 'ATP-dependent chaperone ClpB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (841, 'uncharacterized protein/domain associated withGTPase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (842, 'cyclic peptide transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (843, 'Thioesterase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (844, 'MbtH domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (845, 'Beta-ketoacyl synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (846, '4''-phosphopantetheinyl transferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (847, 'amino acid adenylation domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (848, 'Erythronolide synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (849, 'diaminobutyrate--2-oxoglutarateaminotransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (850, 'phosphatidylserine decarboxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (851, 'helix-hairpin-helix motif', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (852, 'extracellular solute-binding protein family 5', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (853, 'xylanase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (854, 'GAF sensor signal transduction histidine kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (855, 'UTP-GlnB uridylyltransferase, GlnD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (856, 'valyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (857, 'heat shock protein Hsp90', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (858, 'glycogen debranching enzyme GlgX', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (859, 'malto-oligosyltrehalose trehalohydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (860, 'malto-oligosyltrehalose synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (861, 'inner-membrane translocator', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (862, 'Extracellular ligand-binding receptor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (863, '17 kDa surface antigen', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (864, '2''-5'' RNA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (865, 'small multidrug resistance protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (866, 'acetyl-CoA carboxylase, carboxyl transferase,alpha subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (867, 'carbonic anhydrase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (868, 'phage shock protein C, PspC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (869, 'S-adenosylmethionine--tRNA-ribosyltransferase-isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (870, 'peptidase S8 and S53 subtilisin kexin sedolisin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (871, 'Parallel beta-helix repeat', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (872, 'Alpha-L-fucosidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (873, 'peptidase S16 lon domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (874, 'Na+/melibiose symporter and relatedtransporters-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (875, 'chaperone protein DnaK', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (876, 'chaperonin Cpn10', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (877, 'Respiratory-chain NADH dehydrogenase domain 51kDa subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (878, 'phosphate-selective porin O and P', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (879, 'sulfate ABC transporter, periplasmicsulfate-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (880, 'sulfate ABC transporter, inner membrane subunitCysT', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (881, 'sulfate ABC transporter, inner membrane subunitCysW', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (882, 'sulfate ABC transporter, ATPase subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (883, 'TrkA-C domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (884, 'cysteine synthase A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (885, 'cytochrome c oxidase subunit III', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (886, 'cytochrome c oxidase, subunit I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (887, 'cytochrome c oxidase, subunit II', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (888, 'protein of unknown function DUF420', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (889, 'protoheme IX farnesyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (890, 'cytochrome oxidase assembly', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (891, 'AMP nucleosidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (892, 'lipoic acid synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (893, 'lipoate-protein ligase B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (894, 'lysyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (895, 'lipid-A-disaccharide synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (896, 'glutamyl-tRNA reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (897, 'cytochrome c assembly protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (898, 'Fmu (Sun) domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (899, 'DNA polymerase III, alpha subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (900, 'ribosomal protein S21', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (901, 'HhH-GPD family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (902, 'ribonuclease HIII', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (903, 'Ribonuclease H', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (904, 'Amidophosphoribosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (905, 'putative UDP-N-acetylglucosaminediphosphorylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (906, 'tryptophanyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (907, 'phospholipid/glycerol acyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (908, 'cytidylate kinase region', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (909, '3-phosphoshikimate 1-carboxyvinyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (910, 'Prephenate dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (911, 'protein of unknown function DUF55', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (912, 'glycoside hydrolase family 11', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (913, 'protein of unknown function DUF323', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (914, 'protein of unknown function DUF1593', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (915, 'glycoside hydrolase, family 9', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (916, 'deoxyuridine 5''-triphosphate nucleotidohydrolaseDut', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (917, 'BNR repeat, putative', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (918, 'protein of unknown function DUF362', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (919, '6-phosphogluconolactonase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (920, 'GTP cyclohydrolase I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (921, 'protein of unknown function DUF374', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (922, 'Calcium-binding EF-hand-containing protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (923, '4-hydroxythreonine-4-phosphate dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (924, '2-keto-3-deoxygluconate permease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (925, 'thioesterase superfamily protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (926, 'conserved hypothetical protein-putative secretedprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (927, 'putative phosphohistidine phosphatase, SixA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (928, 'D-alanine--D-alanine ligase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (929, '2-dehydropantoate 2-reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (930, 'YD repeat protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (931, 'DNA polymerase beta domain protein region', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (932, 'chromate transporter, chromate ion transporter(CHR) family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (933, 'putative transcriptional regulatory protein NadR(probably AsnC-family)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (934, 'tRNA-Asp', NULL, 5);
INSERT INTO product (id, name, description, piece_type_id) VALUES (935, 'Gluconate 2-dehydrogenase (acceptor)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (936, 'Gluconate transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (937, 'Beta-glucosidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (938, 'periplasmic component of the Tol biopolymertransport system-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (939, 'Luciferase-like monooxygenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (940, 'drug resistance transporter, EmrB/QacAsubfamily', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (941, 'ribonuclease BN', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (942, 'Glycerol-3-phosphate dehydrogenase (NAD(P)(+))', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (943, 'flavoprotein WrbA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (944, 'Homoserine dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (945, 'phosphoribulokinase/uridine kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (946, 'peptidase S26B, signal peptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (947, 'protein of unknown function DUF262', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (948, 'Peptidoglycan glycosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (949, 'Nucleotidyl transferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (950, '3-demethylubiquinone-9 3-methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (951, 'Vitamin K epoxide reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (952, 'Xylan 1,4-beta-xylosidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (953, 'protein of unknown function DUF1348', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (954, 'glycoside hydrolase family 8', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (955, 'NHL repeat containing protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (956, 'protein of unknown function DUF1573', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (957, 'arsenate reductase and related', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (958, 'protein of unknown function DUF419', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (959, 'ribosomal 5S rRNA E-loop binding proteinCtc/L25/TL5', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (960, 'Aminoacyl-tRNA hydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (961, 'ribosomal protein S6', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (962, 'single-strand binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (963, 'NTPase (NACHT family)-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (964, 'transcriptional repressor, LexA family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (965, 'Alpha-galactosidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (966, 'N-acylglucosamine 2-epimerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (967, 'ribosomal protein L9', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (968, 'replicative DNA helicase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (969, 'outer membrane protein assembly complex, YaeTprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (970, 'outer membrane chaperone Skp (OmpH)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (971, 'UDP-3-O-(3-hydroxymyristoyl) glucosamineN-acyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (972, 'orotate phosphoribosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (973, 'myosin heavy chain', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (974, 'Biopolymer transport protein ExbD/TolR', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (975, '2-amino-4-hydroxy-6-hydroxymethyldihydropteridine pyrophosphokinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (976, 'dihydroneopterin aldolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (977, 'dihydropteroate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (978, 'Chorismate binding-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (979, 'aminotransferase class IV', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (980, 'FolC bifunctional protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (981, 'ribosomal protein L35', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (982, 'ribosomal protein L20', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (983, 'phenylalanyl-tRNA synthetase, alpha subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (984, 'phenylalanyl-tRNA synthetase, beta subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (985, 'glutamyl-tRNA(Gln) amidotransferase, C subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (986, 'glutamyl-tRNA(Gln) amidotransferase, A subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (987, 'glutamyl-tRNA(Gln) amidotransferase, B subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (988, 'Superoxide dismutase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (989, 'ferric uptake regulator, Fur family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (990, 'ATP-NAD/AcoX kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (991, 'hemolysin A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (992, 'peptidase M24', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (993, 'protein of unknown function DUF192', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (994, 'protein of unknown function DUF519', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (995, 'conserved hypothetical cytosolic protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (996, 'glutamyl-tRNA synthetase class Ic', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (997, '2-nitropropane dioxygenase NPD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (998, 'protein of unknown function family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (999, 'ATPase associated with various cellularactivities AAA_3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1000, 'Class I peptide chain release factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1001, 'MOSC domain protein beta barrel domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1002, 'protein of unknown function DUF195', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1003, 'stationary-phase survival protein SurE', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1004, 'secreted protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1005, 'Polyribonucleotide nucleotidyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1006, 'ribosomal protein S15', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1007, 'Nickel-transporting ATPase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1008, 'serine/threonine protein kinase with TPRrepeats', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1009, 'Serine O-acetyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1010, 'sulfate adenylyltransferase, large subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1011, 'Sulfate adenylyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1012, 'Phosphoadenylyl-sulfate reductase (thioredoxin)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1013, 'nitrite and sulphite reductase 4Fe-4S region', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1014, 'uroporphyrin-III C-methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1015, 'cobalamin (vitamin B12) biosynthesis CbiXprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1016, 'peptidase M28', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1017, '2C-methyl-D-erythritol 2,4-cyclodiphosphatesynthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1018, 'orotidine 5''-phosphate decarboxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1019, '4-diphosphocytidyl-2C-methyl-D-erythritolkinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1020, 'phosphoenolpyruvate-protein phosphotransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1021, 'putative cytidylate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1022, 'dihydrofolate reductase region', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1023, 'alanine dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1024, 'DNA replication and repair protein RecF', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1025, 'Crossover junction endodeoxyribonuclease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1026, 'sulfate transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1027, 'respiratory-chain NADH dehydrogenase, subunit 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1028, 'NAD-dependent dehydrogenase subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1029, 'hydrogenase, membrane subunit 3-like protein(EchB-like)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1030, 'hydrogenase 4 membrane component (E)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1031, 'NADH ubiquinone oxidoreductase 20 kDa subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1032, 'NADH-ubiquinone oxidoreductase chain 49kDa', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1033, 'Ferrochelatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1034, 'phosphoglycerate mutase,2,3-bisphosphoglycerate-independent', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1035, 'aspartyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1036, 'ammonium transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1037, 'nitrogen regulatory protein P-II', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1038, 'threonyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1039, 'cysteinyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1040, 'Sel1 domain protein repeat-containing protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1041, 'Porphobilinogen synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1042, 'Dihydroorotate oxidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1043, 'protein of unknown function UPF0079', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1044, 'thiamine-monophosphate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1045, 'dihydroorotase, multifunctional complex type', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1046, 'aspartate carbamoyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1047, 'phosphoribosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1048, 'helicase c2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1049, 'protein of unknown function DUF502', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1050, 'protein of unknown function UPF0054', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1051, '(2Fe-2S)-binding domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1052, 'aldehyde oxidase and xanthine dehydrogenasemolybdopterin binding', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1053, 'molybdopterin dehydrogenase FAD-binding', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1054, 'metal dependent phosphohydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1055, '3-oxoacyl-(acyl-carrier-protein) synthase 2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1056, 'Three-deoxy-D-manno-octulosonic-acid transferasedomain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1057, 'DGPFAETKE family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1058, 'Monogalactosyldiacylglycerol synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1059, 'Chloride channel core', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1060, 'phosphate binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1061, 'phosphate ABC transporter, inner membranesubunit PstC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1062, 'phosphate ABC transporter, inner membranesubunit PstA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1063, 'phosphate ABC transporter, ATPase subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1064, 'phosphate uptake regulator, PhoU', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1065, 'tryptophan synthase, alpha subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1066, 'response regulator receiver modulateddiguanylate cyclase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1067, 'tRNA delta(2)-isopentenylpyrophosphatetransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1068, 'arginyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1069, 'oxidoreductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1070, 'Carbonate dehydratase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1071, 'translation elongation factor P', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1072, 'beta-hydroxyacyl-(acyl-carrier-protein)dehydratase FabZ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1073, 'acyl-(acyl-carrier-protein)--UDP-N-acetylglucosamine O-acyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1074, 'Phosphoribosyl-AMP cyclohydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1075, 'protein of unknown function DUF971', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1076, 'cytochrome C family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1077, 'cytochrome c family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1078, 'UDP-N-acetylenolpyruvoylglucosamine reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1079, 'Peptidase M23', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1080, 'Cytochrome b subunit of formatedehydrogenase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1081, 'PAS fold-3 domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1082, 'protein of unknown function UPF0040', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1083, 'S-adenosyl-methyltransferase MraW', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1084, 'UDP-N-acetylmuramyl-tripeptide synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1085, 'UDP-N-acetylmuramoylalanyl-D-glutamyl-2,6-diaminopimelate--D-alanyl-D-alanyl ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1086, 'phospho-N-acetylmuramoyl-pentapeptide-transferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1087, 'UDP-N-acetylmuramoylalanine--D-glutamate ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1088, 'Undecaprenyldiphospho-muramoylpentapeptidebeta-N-acetylglucosaminyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1089, 'D-alanine--D-alanine ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1090, 'Polypeptide-transport-associated domain proteinFtsQ-type', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1091, 'cell division protein FtsA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1092, 'cell division protein FtsZ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1093, 'glutamyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1094, 'RNA polymerase, sigma 54 subunit, RpoN', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1095, 'iron-sulfur cluster assembly accessory protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1096, 'ubiquinone/menaquinone biosynthesismethyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1097, 'histidyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1098, 'cytidine deaminase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1099, '2-amino-3-ketobutyrate coenzyme A ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1100, 'L-threonine 3-dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1101, 'methylated-DNA--protein-cysteinemethyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1102, 'VacB and RNase II family 3''-5'' exoribonuclease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1103, 'glycogen/starch synthase, ADP-glucose type', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1104, 'Deoxyribodipyrimidine photo-lyase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1105, 'protein of unknown function DUF1731', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1106, 'Nucleoside-diphosphate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1107, 'dehydratase, YjhG/YagF family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1108, 'fumarylacetoacetase family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1109, 'branched-chain amino acid aminotransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1110, 'UvrB/UvrC protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1111, 'ATP:guanido phosphotransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1112, '3-isopropylmalate dehydratase, large subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1113, '3-isopropylmalate dehydratase, small subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1114, 'diacylglycerol kinase catalytic region', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1115, 'tyrosyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1116, 'aconitate hydratase 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1117, 'anti-sigma-factor antagonist', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1118, 'protein serine phosphatase with GAF(s)sensor(s)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1119, 'tryptophan synthase, beta subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1120, 'protein of unknown function DUF1732', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1121, 'lipoprotein signal peptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1122, 'transcriptional regulator, TraR/DksA family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1123, 'isoleucyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1124, 'protoporphyrinogen oxidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1125, 'uroporphyrinogen decarboxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1126, 'Coproporphyrinogen oxidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1127, 'Serine-type D-Ala-D-Ala carboxypeptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1128, 'peptide chain release factor 3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1129, 'putative SAM-dependent methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1130, 'Phosphopyruvate hydratase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1131, 'Ankyrin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1132, 'excinuclease ABC, A subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1133, 'membrane protein of unknown function', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1134, 'dihydrolipoamide dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1135, 'CutA1 divalent ion tolerance protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1136, 'SsrA-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1137, 'homoserine kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1138, 'oligopeptide/dipeptide ABC transporter, ATPasesubunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1139, 'glycosyl transferase family 9', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1140, 'lipid A biosynthesis lauroyl (or palmitoleoyl)acyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1141, 'seryl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1142, 'tRNA(Ile)-lysidine synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1143, 'nucleotide sugar dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1144, 'alanine racemase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1145, 'trigger factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1146, 'Endopeptidase Clp', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1147, 'ATP-dependent Clp protease, ATP-binding subunitClpX', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1148, 'glycoside hydrolase family 9', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1149, 'peptidase M42 family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1150, 'phosphoribosylformimino-5-aminoimidazolecarboxamide ribotide isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1151, 'transferase hexapeptide repeat containingprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1152, 'UDP-N-acetylglucosamine 2-epimerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1153, 'WxcM domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1154, 'DJ-1 family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1155, 'recombination protein RecR', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1156, 'deoxyxylulose-5-phosphate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1157, 'exodeoxyribonuclease VII, small subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1158, 'Ste24 endopeptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1159, 'primosomal protein N''', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1160, 'amino-acid N-acetyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1161, 'prolyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1162, 'ATP-dependent Clp protease adaptor protein ClpS', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1163, '6S ribosomal RNA', NULL, 6);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1164, 'DNA polymerase B region', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1165, 'Adenylosuccinate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1166, 'Malate dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1167, 'Mandelate racemase/muconate lactonizing protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1168, 'O-succinylbenzoic acid--CoA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1169, 'PHP domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1170, 'NusA antitermination factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1171, 'ribosome-binding factor A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1172, 'phosphoesterase RecJ domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1173, 'tRNA pseudouridine synthase B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1174, 'riboflavin biosynthesis protein RibF', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1175, 'DNA repair protein RecN', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1176, 'CHRD domain containing protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1177, 'Allophanate hydrolase subunit 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1178, 'urea amidolyase related protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1179, 'LamB/YcsF family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1180, 'imidazoleglycerol phosphate synthase, cyclasesubunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1181, 'H+transporting two-sector ATPase alpha/betasubunit central region', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1182, 'acetate--CoA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1183, 'signal recognition particle protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1184, 'methionyl-tRNA formyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1185, 'parB-like partition protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1186, 'pyruvate dehydrogenase complex dihydrolipoamideacetyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1187, 'Transketolase central region', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1188, 'pyruvate dehydrogenase (acetyl-transferring) E1component, alpha subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1189, 'Fimbrial assembly family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1190, '5''-3'' exonuclease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1191, 'ATP phosphoribosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1192, 'Agmatine deiminase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1193, 'ribosomal protein S1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1194, 'Stearoyl-CoA 9-desaturase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1195, 'Cytochrome-c oxidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1196, 'NADH dehydrogenase (ubiquinone)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1197, 'iron permease FTR1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1198, 'Cystathionine gamma-synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1199, 'glycosyl hydrolase, family 88', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1200, 'Uroporphyrinogen-III decarboxylase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1201, 'Uroporphyrinogen decarboxylase (URO-D)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1202, 'Glutamine--scyllo-inositol transaminase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1203, 'Heparinase II/III family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1204, 'alpha-L-rhamnosidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1205, 'glycosyl hydrolase BNR repeat-containingprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1206, 'dihydrodipicolinate synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1207, 'CheC, inhibitor of MCP methylation', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1208, 'ABC-type branched-chain amino acid transportsystems periplasmic component-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1209, 'two component regulator propeller domainprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1210, '40-residue YVTN family beta-propeller repeatprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1211, 'asparagine synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1212, 'Glycoside hydrolase family 42 domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1213, 'ACT domain-containing protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1214, 'UDP-glucose 4-epimerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1215, 'phosphonopyruvate decarboxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1216, 'DNA repair protein RadC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1217, 'protein of unknown function DUF1037', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1218, 'PpiC-type peptidyl-prolyl cis-trans isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1219, 'transcription-repair coupling factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1220, 'Ribonuclease III', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1221, 'peptidase C1A papain', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1222, 'ketose-bisphosphate aldolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1223, 'DNA topoisomerase III', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1224, 'Alanine--glyoxylate transaminase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1225, 'tetraacyldisaccharide 4''-kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1226, 'peptidase M29 aminopeptidase II', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1227, 'Adenosine deaminase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1228, 'type III restriction protein res subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1229, 'Restriction endonuclease S subunits-likeprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1230, 'N-6 DNA methylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1231, 'pseudouridine synthase, RluA family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1232, 'Ppx/GppA phosphatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1233, 'Uroporphyrin-III C/tetrapyrrole(Corrin/Porphyrin) methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1234, 'Glucose-6-phosphate isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1235, 'dihydroxy-acid dehydratase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1236, 'segregation and condensation protein B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1237, 'chorismate mutase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1238, 'conserved hypothetical protein-putative membraneprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1239, 'Demethylmenaquinone methyltransferase-likeprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1240, 'putative integral membrane protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1241, 'putative membrane protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1242, 'Spermine synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1243, 'membrane bound O-acyl transferase MBOAT familyprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1244, 'Gamma-glutamyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1245, 'Dipeptidyl-peptidase III', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1246, 'hemerythrin-like metal-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1247, 'PA14 domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1248, 'chromosome segregation ATPase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1249, 'Mg chelatase, subunit ChlI', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1250, 'dephospho-CoA kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1251, 'transcription termination factor Rho', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1252, 'twitching motility protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1253, 'Polyphosphate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1254, 'putative PAS/PAC sensor protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1255, 'oligoendopeptidase F', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1256, 'GTP-binding protein YchF', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1257, 'ribosomal protein S16', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1258, 'tRNA (guanine-N1)-methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1259, 'ribosomal protein L19', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1260, 'peptidase S15', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1261, 'Transcriptional regulators-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1262, 'nucleotide-binding protein containing TIR -likeprotein domain-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1263, '2-isopropylmalate synthase/homocitrate synthasefamily protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1264, 'CsbD family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1265, 'K potassium transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1266, 'naphthoate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1267, 'Alpha-L-arabinofuranosidase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1268, 'protein of unknown function DUF88', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1269, 'glutathione-dependent formaldehyde-activatingGFA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1270, 'Ribosomal protein L11 methylase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1271, 'isocitrate dehydrogenase, NADP-dependent', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1272, 'actin/actin family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1273, 'transcriptional regulator', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1274, 'ribosomal protein L27', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1275, 'ribosomal protein L21', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1276, 'TolA protein, putative', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1277, 'DNA-3-methyladenine glycosylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1278, 'DEAD/H associated domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1279, 'DNA ligase I, ATP-dependent Dnl1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1280, 'protein of unknown function UPF0118', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1281, 'glycosyl hydrolase, BNR repeat', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1282, 'protein of unknown function DUF1329', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1283, 'protein of unknown function DUF1302', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1284, 'DNA-directed DNA polymerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1285, 'putative regulatory protein, FmdB family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1286, 'inositol monophosphatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1287, 'protein of unknown function DUF344', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1288, 'ApaG domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1289, 'phosphoribosylaminoimidazolecarboxamideformyltransferase/IMP cyclohydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1290, 'Holliday junction DNA helicase RuvA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1291, 'Dihydrodipicolinate reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1292, 'dihydrodipicolinate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1293, 'malonyl CoA-acyl carrier protein transacylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1294, 'GTP-binding protein LepA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1295, 'RbsD or FucU transport', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1296, 'glycoside hydrolase family 10', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1297, 'alpha-glucosidase, putative', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1298, 'putative beta-xylosidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1299, 'Carboxylesterase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1300, 'glycoside hydrolase family 62', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1301, 'glycosyl transferase family 14', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1302, 'Capsular polysaccharide biosynthesisprotein-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1303, 'acetyltransferase with hexapeptide repeat', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1304, 'HAD-superfamily hydrolase, subfamily IIB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1305, '3-Oxoacyl-(acyl-carrier-protein (ACP)) synthaseIII domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1306, 'trans-2-enoyl-ACP reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1307, 'Beta-hydroxyacyl-(acyl-carrier-protein)dehydratase FabA/FabZ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1308, 'transcriptional regulator, CadC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1309, 'phosphopantetheine-binding', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1310, 'putative signal transduction protein with CBSdomains', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1311, 'Malate dehydrogenase(oxaloacetate-decarboxylating) (NADP(+))', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1312, 'LAO/AO transport system ATPase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1313, 'methylmalonyl-CoA mutase, large subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1314, 'methylmalonyl-CoA epimerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1315, 'Propionyl-CoA carboxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1316, 'sodium ion-translocating decarboxylase, betasubunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1317, 'sodium pump decarboxylase gamma subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1318, 'pyruvate carboxyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1319, 'putative L-fucose isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1320, 'Monosaccharide-transporting ATPase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1321, 'protein TolA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1322, 'dTMP kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1323, 'DNA polymerase III gamma/tau subunits-likeprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1324, 'glycoside hydrolase 15-related', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1325, 'Galacturan 1,4-alpha-galacturonidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1326, 'putative RNA polymerase, sigma-24 subunit, ECFsubfamily', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1327, 'Beta-glucuronidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1328, 'Helicase ATP-dependent domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1329, 'GTP-binding protein TypA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1330, 'glycine cleavage system H protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1331, 'glycine cleavage system T protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1332, 'prolipoprotein diacylglyceryl transferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1333, 'glucosamine--fructose-6-phosphateaminotransferase, isomerizing', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1334, 'anthranilate synthase component I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1335, 'RNA polymerase, sigma 70 subunit, RpoD family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1336, 'Mu Gam family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1337, 'protein of unknown function DUF264', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1338, 'Mu-like protein prophage protein gp29-likeprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1339, 'DNA topoisomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1340, 'transcriptional regulator protein-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1341, 'single-stranded-DNA-specific exonuclease RecJ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1342, 'protein-export membrane protein SecD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1343, 'preprotein translocase, YajC subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1344, 'arginine decarboxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1345, 'GatB/YqeY domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1346, 'amine oxidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1347, 'protein of unknown function DUF326', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1348, 'Phosphoribosylanthranilate isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1349, 'Uroporphyrinogen III synthase HEM4', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1350, 'porphobilinogen deaminase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1351, 'ketol-acid reductoisomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1352, 'acetolactate synthase, small subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1353, 'aspartate-semialdehyde dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1354, 'ErfK/YbiS/YcfS/YnhG family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1355, 'protein serine/threonine phosphatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1356, 'DnaJ-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1357, 'peptidase membrane zinc metallopeptidaseputative', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1358, 'surface antigen (D15)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1359, 'protein of unknown function DUF490', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1360, 'Oligopeptidase A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1361, 'carboxyl-terminal protease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1362, 'protein-(glutamine-N5) methyltransferase,release factor-specific', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1363, 'peptide chain release factor 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1364, 'membrane-bound mannosyltransferase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1365, 'threonine synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1366, 'aspartate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1367, 'protein of unknown function DUF205', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1368, 'D-3-phosphoglycerate dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1369, 'metal-dependent phosphohydrolase (HDsuperfamily)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1370, 'deoxyguanosinetriphosphate triphosphohydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1371, 'acetyl-CoA carboxylase, carboxyl transferase,beta subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1372, 'competence protein F, putative', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1373, 'tRNA pseudouridine synthase A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1374, 'Holliday junction resolvase YqgF', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1375, 'mucin-associated surface protein (MASP)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1376, 'protein of unknown function DUF990', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1377, 'Cytochrome b/b6 domain', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1378, 'Mn2+ and Fe2+ transporters of the NRAMPfamily-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1379, 'glutamate-1-semialdehyde-2,1-aminomutase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1380, 'MazG family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1381, 'Indole-3-glycerol-phosphate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1382, 'ribosomal RNA adenine methylase transferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1383, 'xylulokinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1384, 'NMT1/THI5 like domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1385, 'Ribosomal small subunit Rsm22', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1386, 'MiaB-like tRNA modifying enzyme YliG', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1387, 'Mur ligase middle domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1388, 'Amidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1389, 'Xanthine/uracil/vitamin C permease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1390, 'Adenine deaminase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1391, 'UspA domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1392, 'Mn2+/Fe2+ transporter, NRAMP family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1393, 'iron (metal) dependent repressor, DtxR family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1394, 'Chitinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1395, 'phosphoglucosamine mutase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1396, 'anthranilate phosphoribosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1397, 'Bacitracin resistance protein BacA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1398, 'ribosomal protein L32', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1399, 'fatty acid/phospholipid synthesis protein PlsX', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1400, 'Ribulose-phosphate 3-epimerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1401, 'glycoside hydrolase family 2 TIM barrel', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1402, 'ABC-type Fe3+ transport system periplasmiccomponent-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1403, 'hypoxanthine phosphoribosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1404, 'excinuclease ABC, B subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1405, 'Glu/Leu/Phe/Val dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1406, 'glycosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1407, 'UDP-galactopyranose mutase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1408, 'glycoside hydrolase family 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1409, 'protein of unknown function DUF1458', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1410, 'protein of unknown function DUF1504', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1411, 'ribosomal protein L31', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1412, 'ribosomal protein L36', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1413, 'protein of unknown function DUF179', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1414, 'CMP/dCMP deaminase zinc-binding', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1415, 'putative electron transfer oxidoreductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1416, 'chalcone and stilbene synthase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1417, 'protein of unknown function DUF34', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1418, '3-dehydroquinate dehydratase, type II', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1419, 'phosphatidylglycerophosphatase A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1420, 'CDP-diacylglycerol--glycerol-3-phosphate3-phosphatidyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1421, 'pantetheine-phosphate adenylyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1422, '2-succinyl-6-hydroxy-2,4-cyclohexadiene-1-carboxylic acid synthase/2-oxoglutaratedecarboxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1423, 'isochorismate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1424, 'methionyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1425, 'hydroxymethylbutenyl pyrophosphate reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1426, 'non-canonical purine NTP pyrophosphatase,rdgB/HAM1 family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1427, 'riboflavin biosynthesis protein RibD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1428, 'cobalamin B12-binding domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1429, '3-isopropylmalate dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1430, 'glycogen debranching enzyme', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1431, 'TonB-dependent copper receptor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1432, 'arsenical-resistance protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1433, 'protein tyrosine phosphatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1434, 'glutaminyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1435, 'putative heterodisulfide reductase, C subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1436, 'CoB--CoM heterodisulfide reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1437, 'methyl-viologen-reducing hydrogenase deltasubunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1438, 'nickel-dependent hydrogenase large subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1439, 'hydrogenase maturation protease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1440, 'ATP-dependent DNA helicase RecQ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1441, 'O-acetylhomoserine/O-acetylserine sulfhydrylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1442, 'Isoprenylcysteine carboxyl methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1443, 'protein of unknown function DUF1212', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1444, 'cobyric acid synthase CobQ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1445, 'transport system permease protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1446, 'periplasmic binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1447, 'Cof-like hydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1448, 'Phosphoribosylformylglycinamidine cyclo-ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1449, 'glutamine amidotransferase class-II', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1450, 'phosphoribosylformylglycinamidine synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1451, 'protein of unknown function DUF480', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1452, 'exo-poly-alpha-D-galacturonosidase precursor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1453, 'asparaginyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1454, 'phosphoserine phosphatase/homoserinephosphotransferase bifunctional protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1455, 'sodium/hydrogen exchanger', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1456, 'type II secretion pathway protein XcpT', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1457, 'CopG domain protein DNA-binding domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1458, 'preprotein translocase, SecG subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1459, 'phosphoglucomutase/phosphomannomutasealpha/beta/alpha domain I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1460, 'Septum formation initiator', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1461, 'ATPase associated with various cellularactivities AAA_5', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1462, 'DNA mismatch repair protein MutS', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1463, 'DAHP synthetase I/KDSA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1464, 'maf protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1465, 'metalloendopeptidase, glycoprotease family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1466, 'protein of unknown function DUF185', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1467, 'ribosomal protein L28', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1468, 'acyl carrier protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1469, '3-oxoacyl-(acyl-carrier-protein) reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1470, 'Methenyltetrahydrofolate cyclohydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1471, 'pyrroline-5-carboxylate reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1472, 'spermidine/putrescine ABC transporter ATPasesubunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1473, 'Ornithine carbamoyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1498, 'transketolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1499, 'Mannose-1-phosphate guanylyltransferase (GDP)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1500, 'NADH:flavin oxidoreductase/NADH oxidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1501, 'RND efflux system, outer membrane lipoprotein,NodT family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1502, 'transporter, hydrophobe/amphiphile efflux-1(HAE1) family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1503, 'Bile acid:sodium symporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1504, 'integral membrane protein MviN', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1505, 'regulatory protein, FmdB family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1506, 'Triose-phosphate isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1507, 'Phosphoglycerate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1508, 'glyceraldehyde-3-phosphate dehydrogenase, typeI', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1509, 'glycoside hydrolase family 13 domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1510, 'type I phosphodiesterase/nucleotidepyrophosphatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1511, 'tRNA(5-methylaminomethyl-2-thiouridylate)-methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1512, 'GTP-binding protein Obg/CgtA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1513, 'alanyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1514, '1,4-alpha-glucan branching enzyme', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1515, 'Organic solvent tolerance protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1516, 'chromosome segregation and condensation proteinScpA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1517, '1,4-dihydroxy-2-naphthoateoctaprenyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1518, 'acylphosphatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1519, 'adenosylhomocysteinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1520, 'Methionine adenosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1521, 'Hpt protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1522, 'periplasmic glucan biosynthesis protein MdoG', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1523, 'phage SPO1 DNA polymerase-related protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1524, 'riboflavin synthase, alpha subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1525, 'peptidase S1 and S6 chymotrypsin/Hap', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1526, 'leucyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1527, 'protein of unknown function DUF1009', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1528, 'protein of unknown function DUF151', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1529, 'dihydrouridine synthase DuS', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1530, 'signal peptide peptidase SppA, 67K type', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1531, 'Diaminopimelate epimerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1532, 'acyl-CoA dehydrogenase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1533, 'Electron-transferring-flavoproteindehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1534, 'electron transfer flavoprotein beta subunit-likeprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1535, 'electron transfer flavoprotein alphasubunit-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1536, 'Trans-2-enoyl-CoA reductase (NAD(+))', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1537, 'D-tyrosyl-tRNA(Tyr) deacylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1538, 'glucose-6-phosphate 1-dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1539, 'protein of unknown function UPF0102', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1540, 'putative phytochrome sensor protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1541, 'amino acid permease-associated region', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1542, 'chromosome segregation protein SMC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1543, 'response regulator receiver sensor hybridhistidine kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1544, 'ATPase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1545, 'Fe-S-cluster-containing hydrogenase components1-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1546, 'adenylylsulfate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1547, 'sulfate adenylyltransferase, small subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1548, 'GDP-mannose 4,6-dehydratase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1549, 'C-methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1550, 'GHMP kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1551, 'sugar isomerase family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1552, 'histidinol-phosphate phosphatase family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1553, 'peptidase M10A and M12B matrixin and adamalysin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1554, 'dTDP-4-dehydrorhamnose 3,5-epimerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1555, 'dTDP-glucose 4,6-dehydratase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1556, 'glucose-1-phosphate thymidylyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1557, '8-amino-7-oxononanoate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1558, 'protein of unknown function DUF1730', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1559, 'Shikimate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1560, 'Threonine aldolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1561, 'Rossmann fold nucleotide-binding protein-likeprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1562, 'phosphoribosylamine--glycine ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1563, 'Glycine hydroxymethyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1564, 'carbonic anhydrase/acetyltransferase, isoleucinepatch superfamily', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1565, 'Na+ dependent nucleoside transporter domainprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1566, 'glycogen/starch/alpha-glucan phosphorylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1675, 'alpha-glucan phosphorylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1567, 'DNA mismatch repair protein MutL', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1568, 'argininosuccinate lyase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1569, 'ABC-1 domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1570, 'Nucleotidyltransferase/DNA polymerase involvedin DNA repair-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1571, 'cytochrome c biogenesis protein transmembraneregion', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1572, 'Thioredoxin domain', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1573, 'NADP oxidoreductase coenzyme F420-dependent', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1574, 'redox-active disulfide protein 2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1575, 'von Willebrand factor, type A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1576, 'PEBP family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1577, 'cytochrome c oxidase, cbb3-type, subunit I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1578, 'cytochrome c oxidase, cbb3-type, subunit III', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1579, 'cytochrome c oxidase accessory protein CcoG', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1580, 'Copper-exporting ATPase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1581, 'Xaa-Pro dipeptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1582, 'glycoside hydrolase family 2 immunoglobulindomain protein beta-sandwich', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1583, 'pyruvate, phosphate dikinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1584, 'Pectinesterase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1585, 'ABC-type nitrate/sulfonate/bicarbonate transportsystems periplasmic components-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1586, 'Helix-turn-helix type 11 domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1587, 'DNA-3-methyladenine glycosylase I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1588, 'succinate CoA transferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1589, 'GTP-binding proten HflX', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1590, 'translation initiation factor, aIF-2BI family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1591, 'respiratory-chain NADH dehydrogenase subunit 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1592, 'ABC-type phosphate/phosphonate transport systemperiplasmic component-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1593, 'putative PhnD (ABC-type phosphate/phosphonatetransport system, periplasmic component) family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1594, 'Carotene 7,8-desaturase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1595, 'ATP dependent DNA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1596, 'formate dehydrogenase family accessory proteinFdhD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1597, 'oxidoreductase alpha (molybdopterin) subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1598, 'uridylate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1599, 'ribosome recycling factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1600, 'glutamate 5-kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1601, 'diaminopimelate dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1602, 'CDP-glycerol:poly(glycerophosphate)glycerophosphotransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1603, 'thioredoxin reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1604, 'FeS assembly ATPase SufC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1605, 'FeS assembly protein SufB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1606, 'FeS assembly protein SufD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1607, 'FeS assembly SUF system protein SufT', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1608, 'putative conserved integral membrane protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1609, '2-isopropylmalate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1610, 'protein of unknown function DUF1452', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1611, 'Gluconolactonase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1612, '5,10-methylenetetrahydrofolate reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1613, 'translation initiation factor SUI1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1614, '2-oxoglutarate dehydrogenase, E1 subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1615, '2-oxoglutarate dehydrogenase, E2 subunit,dihydrolipoamide succinyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1616, 'sugar-phosphate isomerase, RpiB/LacA/LacBfamily', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1617, 'phospholipase/carboxylesterase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1618, '3-deoxy-D-manno-octulosonate 8-phosphatephosphatase, YrbI family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1619, 'OstA family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1620, 'HPr kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1621, 'ribonuclease PH', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1622, 'Urease accessory protein UreD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1623, 'urease accessory protein UreG', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1624, 'Urease accessory protein UreF', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1625, 'UreE urease accessory domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1626, 'SEC-C motif domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1627, 'glutamate racemase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1628, 'urease, alpha subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1629, 'urease, beta subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1630, 'urease, gamma subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1631, 'urea ABC transporter, ATP-binding protein UrtE', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1632, 'urea ABC transporter, ATP-binding protein UrtD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1633, 'urea ABC transporter, permease protein UrtC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1634, 'urea ABC transporter, permease protein UrtB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1635, 'succinyl-CoA synthetase, beta subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1636, 'succinyl-CoA synthetase, alpha subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1637, 'RecA-superfamily ATPase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1638, 'integrase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1639, 'peptide chain release factor 2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1640, 'Sec-independent protein translocase, TatCsubunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1641, 'cell surface receptor IPT/TIG domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1642, 'sulfotransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1643, 'Tail Collar domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1644, 'aspartyl/asparaginyl beta-hydroxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1645, 'Malate/L-lactate dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1646, '2-dehydro-3-deoxyphosphogluconatealdolase/4-hydroxy-2-oxoglutarate aldolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1647, '4-deoxy-L-threo-5-hexosulose-uronateketol-isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1648, '2-deoxy-D-gluconate 3-dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1649, 'Glucuronate isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1650, 'peptidase T2 asparaginase 2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1651, 'DNA polymerase LigD, polymerase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1652, 'DNA polymerase LigD, ligase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1653, 'Alpha-glucuronidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1654, 'Indolepyruvate ferredoxin oxidoreductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1655, 'pyruvate ferredoxin/flavodoxin oxidoreductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1656, 'Phenylacetate--CoA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1657, 'ACT domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1658, 'protein of unknown function DUF1697', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1659, 'Chondroitin AC lyase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1660, 'homocysteine S-methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1661, 'dihydropteroate synthase DHPS', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1662, 'methylenetetrahydrofolate reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1663, 'Mannonate dehydratase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1664, 'protein of unknown function DUF1469', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1665, 'sugar isomerase (SIS)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1666, 'Domain of unknown function DUF1846', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1667, 'Domain of unknown function DUF1814', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1668, 'protein of unknown function DUF534', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1669, 'protein of unknown function DUF305', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1670, 'Glucose/sorbosone dehydrogenase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1671, 'copper-resistance protein, CopA family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1672, 'copper resistance B precursor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1673, 'protein of unknown function DUF355', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1674, 'protein of unknown function, Spy-related', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1676, 'purine or other phosphorylase family 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1677, 'DsrE family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1678, 'ribosomal protein S2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1679, 'translation elongation factor Ts', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1680, 'hydrogenase assembly chaperone hypC/hupF', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1681, 'hydrogenase expression/formation protein HypD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1682, 'hydrogenase expression/formation protein HypE', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1683, 'hydrogenase nickel insertion protein HypA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1684, 'hydrogenase accessory protein HypB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1685, '(NiFe) hydrogenase maturation protein HypF', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1686, 'sigma-70 factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1687, 'nuclear protein SET', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1688, 'protein of unknown function DUF885', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1689, 'Carbohydrate binding family 6', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1690, 'CrcB protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1691, 'protein of unknown function DUF190', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1692, 'translation initiation factor IF-3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1693, '3-dehydroquinate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1694, 'TonB-dependent siderophore receptor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1695, 'PepSY-associated TM helix domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1696, 'Glutamate--ammonia ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1697, 'Mn2+-dependent serine/threonine protein kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1698, 'AIG2 family protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1699, 'DNA polymerase III, subunits gamma and tau', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1700, 'drug resistance transporter, Bcr/CflA subfamily', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1701, 'NAD(P)(+) transhydrogenase (AB-specific)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1702, 'putative NAD(P) transhydrogenase subunit alpha', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1703, 'alanine dehydrogenase/PNT domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1704, 'AIR synthase related protein domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1705, 'tyrosine recombinase XerD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1706, 'Periplasmic component of the Tol biopolymertransport system-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1707, 'ribosomal protein L33', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1708, 'protein of unknown function DUF147', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1709, 'DNA primase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1710, 'cell shape determining protein, MreB/Mrl family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1711, 'Rod shape-determining protein MreC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1712, 'protein of unknown function DUF58', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1713, '5-oxopent-3-ene-1,2,5-tricarboxylatedecarboxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1714, 'cytochrome c nitrate reductase, small subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1715, 'Nitrite reductase (cytochrome; ammonia-forming)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1716, 'oligopeptide transporter, OPT family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1717, 'oligopeptide transporter, OPT superfamily', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1718, 'Endothelin-converting enzyme 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1719, 'Glucan endo-1,3-beta-D-glucosidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1720, 'protein of unknown function DUF330', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1721, 'helicase, RecD/TraA family', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1722, '1-deoxy-D-xylulose 5-phosphate reductoisomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1723, 'membrane-associated zinc metalloprotease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1724, '1-hydroxy-2-methyl-2-(E)-butenyl 4-diphosphatesynthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1, '18S ribosomal RNA', '18S рибосомальная РНК', 6);
INSERT INTO product (id, name, description, piece_type_id) VALUES (3, '16S ribosomal RNA', '16S рибосомальная РНК', 6);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1725, '50S ribosomal protein L34', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1726, '50S ribosomal protein L35', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1727, '50S ribosomal protein L25', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1728, 'transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1729, 'Chromosomal replication initiator protein DnaA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1730, 'Tetratricopeptide repeat-containing protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1731, 'UDP-N-acetylmuramoyl-L-alanyl-D-glutamate--2,6-diaminopimelate ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1732, 'Phospho-N-acetylmuramoyl-pentapeptide-transferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1733, 'succinyl-CoA:3-ketoacid-coenzyme Atransferasesubunit B (Succinyl CoA:3-oxoacid CoA-transferase)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1734, 'Protein MurJ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1735, 'Cytochrome c-type biogenesis protein CcmE', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1736, 'Protein translocase subunit SecD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1737, 'HAD-superfamily subfamily IIA hydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1738, 'UDP-N-acetylglucosamine1-carboxyvinyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1739, 'Holo-[acyl-carrier-protein] synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1740, 'Protein translocase subunit SecA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1741, 'Sodium/pantothenate symporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1742, 'Protein MraZ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1743, 'Cell division protein FtsL', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1744, 'response regulator NtrX-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1745, 'UPF0192 protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1746, 'Tyrosyl-tRNA synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1747, 'Transcription elongation protein NusA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1748, 'DNA repair protein RecO', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1749, 'DNA repair protein RadA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1750, 'RNA pseudouridine synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1751, 'aromatic acid decarboxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1752, 'Delta-aminolevulinic acid dehydratase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1753, 'reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1754, 'Folylpolyglutamate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1755, 'hydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1756, 'Dihydrolipoyllysine-residue acetyltransferasecomponent of pyruvate dehydrogenase complex', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1757, 'Single-stranded-DNA-specific exonuclease RecJ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1758, 'Transcription termination factor Rho', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1759, 'DNA-binding protein HU-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1760, '30S ribosomal protein S1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1766, 'Sigma(54) modulation protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1767, 'Bifunctional protein FolD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1768, 'Ribonucleoside-diphosphate reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1769, 'tRNA dimethylallyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1770, 'ABC transporter ATP-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1771, 'Antigenic heat-stable 120 kDa protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1772, 'Ribonuclease BN', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1773, 'Laccase domain protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1774, 'Pyruvate, phosphate dikinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1775, 'Cysteine desulfurase protein IscS/NifS', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1776, 'NFS1 nitrogen fixation 1 isoform CRA_b', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1777, 'Aminopeptidase P', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1778, 'Lipopolysaccharide 1,2-glucosyltransferase RfaJ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1779, 'transporter AmpG 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1780, '1-acyl-sn-glycerol-3-phosphate acyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1781, 'Porphobilinogen deaminase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1782, 'Glycine cleavage T-protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1783, 'Ribonuclease D', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1784, 'Dihydrolipoyl dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1785, 'Soluble lytic murein transglycosylase-likeregulatory protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1786, 'transcriptional regulatory protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2048, '30S ribosomal protein S19', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1787, 'Isopentenyl-diphosphate delta-isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1788, 'Lon protease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1789, 'Glycosyl transferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1790, '2-hydroxy-6-oxo-6-phenylhexa-2,4-dienoatehydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1791, 'Glycerol-3-phosphate dehydrogenase [NAD(P)+]', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1792, 'inorganic polyphosphate/ATP-NAD kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1793, 'Recombination protein RecR', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1794, 'Succinyl-CoA ligase [ADP-forming] subunitalpha-2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1795, 'branched-chain-amino-acid aminotransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1796, 'Osmolarity sensor protein EnvZ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1797, 'Phosphatidate cytidylyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1798, 'Phenylalanine--tRNA ligase beta subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1799, 'Threonylcarbamoyladenosine tRNAmethylthiotransferase MtaB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1800, 'Glycosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1801, 'UDP-N-acetylglucosamine--N-acetylmuramyl-(pentapeptide) pyrophosphoryl-undecaprenolN-acetylglucosamine transferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1802, 'Lipoprotein signal peptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1803, 'cytochrome c oxidase subunit 2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1804, 'glutamine amidotransferase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1805, 'carboxypeptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1806, 'Soluble lytic murein transglycosylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1807, 'protease SOHB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1808, 'dioxygenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1809, 'Periplasmic protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1810, 'Penicillin-binding protein dacF', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1811, 'Multidrug resistance protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1812, 'Holliday junction ATP-dependent DNA helicaseRuvA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1813, 'Proline--tRNA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1814, 'Proline/betaine transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1815, 'NADP-dependent malic enzyme', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1816, 'Lysine--tRNA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1817, 'Apolipoprotein N-acyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1818, '3-methyladenine DNA glycosylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1819, 'Outer membrane assembly protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1820, '30S ribosomal protein S4', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1821, 'Alpha/beta hydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1822, 'RND family efflux transporter MFP subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1823, 'Integral membrane protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1824, 'Thioredoxin peroxidase 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1825, 'DNA topoisomerase 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1826, 'Aminodeoxychorismate lyase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1827, 'ATP-dependent protease ATPase subunit HslU', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1828, 'Aspartate-semialdehyde dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1829, 'HlyD family secretion protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1830, 'Guanosine polyphosphatepyrophosphohydrolase/synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1831, 'Transport protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1832, 'Amino acid permease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1833, 'RNA polymerase sigma-32 factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1834, 'Thymidylate synthase ThyX', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1835, 'sugar phosphate isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1836, '3''-phosphoadenosine 5''-phosphosulfate3''-phosphatase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1837, 'Type IV secretion system protein VirD4', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1838, 'VirB10 protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1839, 'VirB9', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1840, 'monovalent cation/H+ antiporter subunit C', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1841, 'NADH dehydrogenase I chain L', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1842, 'Rod shape-determining protein RodA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1843, 'Translation factor GUF1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1844, 'Small heat shock protein C1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1845, 'Cytochrome b', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1846, 'Multisubunit Na+/H+ antiporter, MnhB subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1847, 'Isocitrate dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1848, 'Pyruvate dehydrogenase E1 component subunitalpha', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1849, 'Penicillin binding protein 4*', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1850, 'Heme A synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1851, 'Ribonuclease E', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1852, 'UDP-3-O-[3-hydroxymyristoyl] N-acetylglucosaminedeacetylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1853, 'Cell division protein FtsQ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1854, 'Phosphatidylserine decarboxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1855, 'Elongation factor P', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1856, 'Regulatory component of sensory transductionsystem', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1857, '30S ribosomal protein S9', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1858, 'Tol system periplasmic component', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1859, 'Carboxyl-terminal protease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1860, 'DNA topoisomerase 4 subunit B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1861, 'Threonine--tRNA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1862, 'zinc protease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1863, 'Cytochrome d ubiquinol oxidase, subunit II', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1864, 'S-adenosylmethionine:tRNAribosyltransferase-isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1865, 'ATPase N2B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1866, 'Methionyl-tRNA formyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1867, 'DNA gyrase subunit A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1868, 'Glutaredoxin-1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1869, 'Ribonuclease HII', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1870, 'Chaperone protein HscA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1871, 'Universal stress protein UspA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1872, 'Bifunctional penicillin-binding protein 1C', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1873, 'Cytochrome c oxidase subunit 3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1874, 'Ubiquinone biosynthesis protein Coq7', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1875, 'protease DO', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1876, 'Chaperone protein DnaJ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1877, '2-oxoglutarate dehydrogenase E1 component', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1878, 'Dihydrolipoyllysine-residue succinyltransferasecomponent of 2-oxoglutarate dehydrogenase complex', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1879, 'Periplasmic divalent cation tolerance protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1880, 'Na+-H+-dicarboxylate symporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1881, 'peptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1882, 'DNA-binding protein HU', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1883, 'Hydrophobe/amphiphile efflux-1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1884, 'Ribosomal RNA small subunit methyltransferase E', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1885, 'Ribosome association toxin RatA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1886, 'Ribosomal RNA large subunit methyltransferase E', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1887, 'zinc metalloprotease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1888, 'Multisubunit Na+/H+ antiporter, MnhE subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1889, 'Ribosome-recycling factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1890, 'Glutamyl-tRNA(Gln) amidotransferase subunit A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1891, 'Amino acid ABC transporter substrate bindingprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1892, 'Cytosol aminopeptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1893, 'DNA-directed RNA polymerase subunit beta', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1894, '50S ribosomal protein L7/L12', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1895, '50S ribosomal protein L1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1896, 'Transcription antitermination protein NusG', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1897, 'Elongation factor G', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1898, '30S ribosomal protein S12', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1899, 'Succinate dehydrogenase [ubiquinone]flavoprotein subunit 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1900, 'Succinate dehydrogenase cytochrome b556 subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1901, 'periplasmic serine endoprotease DegP-likeprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1902, 'HflK protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1903, 'GTPase Era', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1904, 'Signal peptidase I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1905, 'Protein translocase subunit SecF', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1906, '50S ribosomal protein L19', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1907, 'Acetate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1908, 'lipoprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1909, 'VirB4', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1910, 'GTP-binding protein EngB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1911, '50S ribosomal protein L28', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1912, 'Ribonucleotide ABC transporter ATP-bindingprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1913, 'Alanine racemase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1914, 'ABC-type transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1915, '30S ribosomal protein S2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1916, '190 kDa antigen', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1917, 'outer membrane protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1918, 'Transcriptional activator protein CzcR', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1919, 'Deoxycytidine triphosphate deaminase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1920, 'DNA topoisomerase 4 subunit A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1921, 'Arginine--tRNA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1922, 'chromosome-partitioning protein ParB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1923, 'Ribosomal RNA small subunit methyltransferase G', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1924, 'Nucleoside diphosphate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1925, 'ADP,ATP carrier protein 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1926, 'Membrane protein insertase YidC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1927, 'Prolipoprotein diacylglyceryl transferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1928, 'Succinate dehydrogenase [ubiquinone] iron-sulfursubunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1929, 'tRNA(Ile)-lysidine synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1930, '30S ribosomal protein S18', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1931, 'Acyl-CoA desaturase 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1932, 'Chaperone protein ClpB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1933, 'UPF0301 protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1934, 'SCO2 protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1935, 'Transcriptional regulator', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1936, 'ATP synthase subunit c', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1937, 'ATP synthase subunit b', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1938, '3-hydroxyacyl-[acyl-carrier-protein] dehydrataseFabZ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1939, 'NADPH-dependent glutamate synthase beta chain', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1940, 'O-antigen export system ATP-binding proteinRfbE', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1941, 'pyruvate, phosphate dikinase regulatory protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1942, 'Coproporphyrinogen-III oxidase, aerobic', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1943, '50S ribosomal protein L33', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1944, 'Octanoyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1945, 'Succinyl-diaminopimelate desuccinylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1946, 'Nucleoid-associated protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1947, 'NAD(P) transhydrogenase subunit alpha part 2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1948, 'RNA polymerase sigma factor RpoD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1949, 'Alanine--tRNA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1950, 'Proline/betaine transporter ProP6', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1951, 'Glycine--tRNA ligase alpha subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1952, 'Sua5', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1953, 'Pseudouridine synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1954, 'GTPase obg', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1955, '5-aminolevulinate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1956, 'MFS type sugar transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1957, 'Single-stranded DNA-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1958, 'Zinc import ATP-binding protein ZnuC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1959, 'Protein p34', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1960, 'Heme exporter protein C', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1961, 'Parvulin-like peptidyl-prolyl isomerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1962, 'Phospholipase D', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1963, 'Tyrosine recombinase XerC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1964, 'Maf-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1965, 'Glutathione-regulated potassium-efflux systemprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1966, '(Dimethylallyl)adenosine tRNAmethylthiotransferase MiaB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1967, 'Strees induced DNA-binding Dps', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1968, 'ATP synthase subunit delta', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1969, 'ATP synthase gamma chain', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1970, 'ATP synthase epsilon chain', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1971, 'NADH dehydrogenase subunit G', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1972, 'Cytochrome c biogenesis ATP-binding exportprotein CcmA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1973, 'Serine--tRNA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1974, 'transporter AmpG 3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1975, 'DNA polymerase III subunit alpha', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1976, 'DNA polymerase I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1977, 'TmRNA-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1978, '3-oxoacyl-[acyl-carrier-protein] synthase 3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1979, 'Permease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1980, 'Cell shape-determining protein MreC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1981, 'HTH-type transcriptional regulator', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1982, '3-oxoacyl-[acyl-carrier-protein] synthase 2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1983, 'Bacterial NAD-glutamate dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1984, 'Aspartokinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1985, '50S ribosomal protein L21', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1986, 'Sec-independent protein translocase proteinTatA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1987, 'Ribosomal RNA small subunit methyltransferase I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1988, 'Endonuclease III', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (1989, 'Lipoyl synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2016, 'Hemolysin C', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2017, 'Poly-beta-hydroxybutyrate polymerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2018, 'Malonyl CoA-acyl carrier protein transacylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2019, 'SURF1-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2020, 'Dephospho-CoA kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2021, 'DNA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2022, 'Lipid A biosynthesis lauroyl acyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2023, 'Ankyrin repeat-containing protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2024, 'transposase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2025, 'ankyrin repeat protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2026, 'resolvase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2027, 'Transposase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2028, 'Outer membrane protein B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2029, 'Magnesium and cobalt efflux protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2030, 'Lipoprotein-releasing system ATP-binding proteinLolD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2031, 'Bicyclomycin resistance protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2032, 'export ATP-binding/permease protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2033, 'Ribosomal-protein-alanine acetyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2034, 'LicD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2035, 'Valine--tRNA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2036, 'Methionine--tRNA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2037, 'Poly-beta-hydroxyalkanoate depolymerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2038, '2-polyprenylphenol 6-hydroxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2039, 'Exodeoxyribonuclease III', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2040, 'Exodeoxyribonuclease 7 large subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2041, 'Cold shock-like protein CspA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2042, 'Muropeptide permease AmpG', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2043, 'Cell division protein FtsZ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2044, 'tRNA/rRNA methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2049, '30S ribosomal protein S3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2050, '50S ribosomal protein L29', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2051, '50S ribosomal protein L14', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2052, '50S ribosomal protein L5', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2053, '30S ribosomal protein S8', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2054, '50S ribosomal protein L18', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2055, '50S ribosomal protein L30', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2056, 'Protein translocase subunit SecY', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2057, '30S ribosomal protein S13', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2058, 'DNA-directed RNA polymerase subunit alpha', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2059, '30S ribosomal protein S20', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2060, 'UPF0118 membrane protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2061, 'Ribonuclease PH', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2062, '60 kDa chaperonin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2063, 'Guanosine-3'',5''-bis(Diphosphate)3''-pyrophosphohydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2064, '2-acylglycerophosphoethanolamineacyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2065, 'Propionyl-CoA carboxylase beta chain', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2066, 'sensor histidine kinase NtrY-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2067, 'Aspartyl/glutamyl-tRNA(Asn/Gln) amidotransferasesubunit C', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2068, 'Aspartyl/glutamyl-tRNA(Asn/Gln) amidotransferasesubunit B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2069, 'Flavoprotein oxygenase DIM6/NTAB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2070, 'Aspartate--tRNA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2071, 'Chromosome partitioning protein-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2072, '50S ribosomal protein L10', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2073, '50S ribosomal protein L11', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2074, 'Preprotein translocase subunit SecE', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2075, '30S ribosomal protein S7', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2076, 'glutamine transport system permease proteinGlnP', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2077, 'Succinate dehydrogenase hydrophobic membraneanchor subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2078, 'HflC protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2079, 'Protein mrp', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2080, 'Crossover junction endodeoxyribonuclease RuvC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2081, 'Ribonuclease 3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2082, 'UPF0335 protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2083, 'tRNA (guanine-N(1)-)-methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2084, 'Phosphate acetyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2085, 'Type IV secretion system protein VirB3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2086, '50S ribosomal protein L31', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2087, 'ABC transporter permease protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2088, 'Mechanosensitive ion channel', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2089, 'metalloprotease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2090, '50S ribosomal protein L9', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2091, '30S ribosomal protein S6', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2092, 'tRNA threonylcarbamoyladenosine biosynthesisprotein Gcp', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2093, 'Acetoacetyl-CoA reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2094, 'protein-disulfide oxidoreductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2095, 'ATP synthase subunit a', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2096, 'ATP synthase B chain', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2097, 'Poly(A) polymerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2098, 'ATP-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2099, 'tRNA-dihydrouridine synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2100, 'UDP-3-O-acylglucosamine N-acyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2101, 'Acyl-[acyl-carrier-protein]--UDP-N-acetylglucosamine O-acyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2102, 'O-antigen export system permease protein RfbA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2103, 'Thioredoxin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2104, 'Uroporphyrinogen decarboxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2105, 'UPF0093 membrane protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2106, '30S ribosomal protein S16', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2107, 'Glutamine ABC transporter ATP-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2108, 'DNA polymerase III subunits gamma and tau', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2109, 'NAD(P) transhydrogenase subunit alpha part 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2110, 'Transcription elongation factor GreA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2111, 'Citrate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2112, 'Trigger factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2113, 'Heat shock protein HtpG', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2114, 'Single-strand DNA-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2115, 'UvrABC system protein A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2116, 'deaminase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2117, 'Ferredoxin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2118, 'adhesin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2119, 'Undecaprenyl-phosphatealpha-N-acetylglucosaminyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2120, 'Methionine aminopeptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2121, 'DNA translocase FtsK', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2122, 'Dihydrolipoamide dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2123, 'ATP synthase subunit alpha', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2124, 'ATP synthase subunit beta', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2125, 'Aconitate hydratase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2126, 'UDP-glucose 6-dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2127, 'S-adenosylmethionine synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2128, 'Acyl carrier protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2129, 'Protein RecA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2130, 'tRNA modification GTPase MnmE', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2131, 'Glutaredoxin-like protein grla', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2132, 'Serine hydroxymethyltransferase 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2133, 'rRNA maturation factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2134, 'ADP,ATP carrier protein 5', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2135, 'Acetyl-CoA acetyltransferase (Beta-ketothiolase)protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2136, 'ATP-dependent helicase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2137, 'DNA polymerase III subunit epsilon', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2138, 'ABC transporter substrate binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2139, 'Queuine tRNA-ribosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2140, 'Tetraacyldisaccharide 4''-kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2141, 'Histone-like DNA-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2142, 'Cytochrome c-type biogenesis protein ccmF', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2143, 'Ribosomal RNA small subunit methyltransferase A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2144, '30S ribosomal protein S10', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2145, '50S ribosomal protein L4', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2146, '50S ribosomal protein L2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2147, '50S ribosomal protein L22', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2148, '50S ribosomal protein L16', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2149, '30S ribosomal protein S17', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2150, '50S ribosomal protein L24', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2151, '30S ribosomal protein S14', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2152, '50S ribosomal protein L6', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2153, '30S ribosomal protein S5', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2154, '50S ribosomal protein L15', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2155, 'Adenylate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2156, '30S ribosomal protein S11', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2157, '50S ribosomal protein L17', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2158, '10 kDa chaperonin', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2159, 'Isoleucine--tRNA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2160, '30S ribosomal protein S21', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2161, 'Ribonuclease P protein component', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2162, '50S ribosomal protein L20', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2163, '7-carboxy-7-deazaguanine synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2164, 'Peptidyl-tRNA hydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2165, 'GTP-dependent nucleic acid-binding protein EngD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2166, 'Patatin-like phospholipase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2167, 'Transcription-repair-coupling factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2168, 'DNA gyrase subunit B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2169, 'DNA-directed RNA polymerase subunit omega', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2170, 'Parvulin-like PPIase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2171, 'UvrABC system protein C', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2172, 'Penicillin-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2173, 'low-complexity protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2174, 'UbiH protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2175, 'fatty acid oxidation complex trifunctionalenzyme', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2176, 'Tyrosine--tRNA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2177, 'Ribosome maturation factor RimP', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2178, 'Translation initiation factor IF-2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2179, 'Superoxide dismutase (Mn/Fe)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2180, 'Biotin--protein ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2181, 'Translation initiation factor IF-3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2182, 'Peptide chain release factor 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2183, 'Methyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2184, 'Signal peptide peptidase SppA, 36K type', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2185, 'Cytidylate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2186, 'ATP-dependent Clp protease proteolytic subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2187, 'Carbonicanhydrase/acetyltransferase,isoleucinepatch super', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2188, 'Ferredoxin--NADP reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2189, 'Phosphomannomutase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2190, '30S ribosomal protein S15', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2191, 'ADP,ATP carrier protein 4', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2192, 'Glutamine synthetase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2193, 'Octaprenyl-diphosphate synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2194, 'ADP,ATP carrier protein 3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2195, 'Tryptophan--tRNA ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2196, 'Alkaline phosphatase synthesis sensor proteinPhoR', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2197, '5-formyltetrahydrofolate cyclo-ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2198, 'UDP-N-acetylglucosamine pyrophosphorylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2199, 'Cell surface antigen', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2200, 'Threonine dehydratase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2201, 'DNA helicase II', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2202, 'Thioredoxin reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2203, 'Metallo-beta-lactamase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2204, 'Ribosome-binding factor A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2205, 'Cell division protein FtsW', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2206, 'Membrane-bound metallopeptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2207, 'Cytochrome c oxidase subunit 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2208, 'polysaccharide polymerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2209, 'membrane protein insertion efficiency factor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2210, 'Exodeoxyribonuclease 7 small subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2211, 'Ribosome maturation factor RimM', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2212, 'Protoheme IX farnesyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2213, 'WaaG-like sugar transferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2214, 'Holliday junction resolvase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2215, 'Glutamate--tRNA ligase 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2216, 'Periplasmic protein TonB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2217, 'TolQ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2218, 'Protease II', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2219, 'Peptide chain release factor 2', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2220, 'Cytochrome c1, heme protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2221, 'UPF0091 protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2222, 'Outer membrane protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2223, 'Phosphoribosylaminoimidazole-succinocarboxamidesynthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2224, 'Cytochrome D ubiquinol oxidase subunit 1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2225, 'Multidrug resistance ABC transporter ATP-bindingprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2257, 'Stress-70 protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2258, 'Outer membrane protein assembly factor BamD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2259, 'Thermostable carboxypeptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2260, '6-carboxy-5,6,7,8-tetrahydropterin synthase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2261, 'DNA polymerase III subunit delta''', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2262, 'UPF0369 protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2263, 'N utilization substance protein B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2264, 'Outer membrane protein assembly factor BamA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2265, 'MFS-type multidrug resistance protein B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2266, 'Uridylate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2267, 'O-antigen export system permease RfbA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2268, 'bifunctional glutamate synthase subunitbeta/2-polyprenylphenol hydroxylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2269, 'UDP-N-acetylglucosamine acyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2270, '(3R)-hydroxymyristoyl-ACP dehydratase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2271, 'UDP-3-O-[3-hydroxymyristoyl] glucosamineN-acyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2272, 'nifR3-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2273, 'zinc/manganese ABC transporter substrate-bindingprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2274, 'poly(A) polymerase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2275, 'competence locus E protein 3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2276, 'ATP synthase F0F1 subunit B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2277, 'ATP synthase F0F1 subunit B''', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2278, 'ATP synthase F0F1 subunit C', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2279, 'ATP synthase F0F1 subunit A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2312, 'recombination protein F', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2313, 'coenzyme PQQ synthesis protein c', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2314, 'dihydrofolate reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2315, 'folate synthesis bifunctional protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2316, 'sco2 protein precursor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2317, 'acetoacetyl-CoA reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2318, 'clpB protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2319, 'DNA-binding/iron metalloprotein/AP endonuclease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2320, 'acyl-CoA desaturase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2321, 'cell cycle protein MesJ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2322, 'cell division protein ftsH', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2323, 'succinate dehydrogenase iron-sulfur subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2324, 'inner membrane protein translocase componentYidC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2325, 'BioC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2326, 'polypeptide deformylase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2327, 'ADP,ATP carrier protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2328, 'glycerol-3-phosphate transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2329, 'nucleoside diphosphate kinase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2330, 'tRNA uridine 5-carboxymethylaminomethylmodification enzyme GidA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2331, '16S rRNA methyltransferase GidB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2332, 'soj protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2333, 'stage 0 sporulation protein J', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2334, 'cation efflux system protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2335, 'hesB protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2336, 'deoxyguanosinetriphosphatetriphosphohydrolase-like protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2337, 'DNA topoisomerase IV subunit A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2338, 'deoxycytidine triphosphate deaminase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2339, 'preprotein translocase subunit SecB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2340, 'transcriptional activator protein czcR', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2341, 'NAD(P) transhydrogenase subunit beta', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2342, 'proline/betaine transporter', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2343, 'preprotein translocase subunit SecG', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2344, 'elongation factor Ts', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2345, '3-deoxy-D-manno-octulosonic-acid transferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2346, 'aspartate aminotransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2347, 'vacJ lipoprotein precursor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2348, 'ABC transporter permease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2349, 'ribonucleotide ABC transporter ATP-bindingprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2350, 'ribosome biogenesis GTP-binding protein YsxC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2351, 'type IV secretion system protein VirB3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2352, 'type IV secretion system ATPase VirB4', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2353, 'phosphate acetyltransferase (Pta)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2354, 'acetate kinase (AckA)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2355, 'preprotein translocase subunit SecF', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2356, 'NADH dehydrogenase I subunit F', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2357, 'ribonuclease III', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2358, 'GTP-binding protein Era', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2359, 'Mrp protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2360, 'protease activity modulator HflK', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2361, 'hflC protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2362, 'periplasmic serine protease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2363, 'succinate dehydrogenase cytochrome b-556subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2364, 'succinate dehydrogenase hydrophobic membraneanchor protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2365, 'succinate dehydrogenase flavoprotein subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2366, 'amino acid ABC transporter permease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2367, 'elongation factor G', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2368, 'preprotein translocase subunit SecE', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2369, 'transcription antitermination protein NusG', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2370, 'DNA-directed RNA polymerase subunit beta''', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2371, 'leucyl aminopeptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2372, 'dihydrodipicolinate reductase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2373, 'amino acid ABC transporter substrate-bindingprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2374, 'aspartyl/glutamyl-tRNA amidotransferase subunitB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2375, 'aspartyl/glutamyl-tRNA amidotransferase subunitA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2376, 'aspartyl/glutamyl-tRNA amidotransferase subunitC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2377, 'monovalent cation/H+ antiporter subunit E', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2378, 'multidrug resistance protein B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2379, 'transcription antitermination protein NusB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2380, 'cell division protein ftsJ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2381, '16S ribosomal RNA methyltransferase RsmE', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2382, 'acriflavin resistance protein D', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2383, 'prolyl endopeptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2384, 'coproporphyrinogen III oxidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2385, 'proton/sodium-glutamate symport protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2386, 'periplasmic divalent cation tolerance protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2387, 'dihydrolipoamide succinyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2388, '2-oxoglutarate dehydrogenase E1', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2389, 'thermostable carboxypeptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2390, 'cation transport regulator ChaB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2391, 'molecular chaperone DnaJ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2392, 'molecular chaperone DnaK', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2393, 'heat shock protease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2394, 'DNA polymerase III subunit delta', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2395, 'ubiquinone biosynthesis protein coq7', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2396, '2,3,4,5-tetrahydropyridine-2,6-carboxylateN-succinyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2397, 'bifunctional penicillin-binding protein 1C', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2398, 'chaperone protein HscA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2399, 'co-chaperone HscB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2400, 'ribonuclease HII', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2401, 'excinuclease ABC subunit B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2402, 'glutaredoxin 3', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2403, 'multidrug resistance protein (Atm1)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2404, 'ATPase n2B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2405, 'multidrug resistance ABC transporter ATP-bindingprotein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2406, 'cytochrome d ubiquinol oxidase subunit I', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2407, 'cytochrome d ubiquinol oxidase subunit II', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2408, 'beta 1,4 glucosyltransferase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2409, 'mitochondrial protease', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2410, 'outer membrane protein TolC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2411, 'DNA topoisomerase IV subunit B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2412, 'tail-specific protease precursor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2413, 'histidine kinase sensor protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2414, '50S ribosomal protein L13', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2415, 'D-alanyl-D-alanine dipeptidase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2416, 'dinucleoside polyphosphate hydrolase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2417, 'response regulator PleD', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2418, 'elongation factor P', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2419, 'extragenic suppressor protein suhB', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2420, 'multidrug resistance protein A', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2421, 'morphology/transcriptional regulatory proteinBolA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2422, 'UDP-N-acetylmuramate--L-alanine ligase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2423, 'cell division protein FtsQ', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2424, 'cell division protein ftsA', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2425, 'cytochrome c', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2426, 'ribonuclease E', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2427, 'cytochrome c oxidase assembly protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2428, 'ribosomal large subunit pseudouridine synthaseC', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2429, 'penicillin-binding protein 4*', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2430, 'exodeoxyribonuclease III', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2431, 'pyruvate dehydrogenase e1 component, alphasubunit precursor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2432, 'pyruvate dehydrogenase subunit beta', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2433, 'GTP-binding protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2434, 'isocitrate dehydrogenase', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2435, 'monovalent cation/H+ antiporter subunit G', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2436, 'monovalent cation/H+ antiporter subunit B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2437, 'heme exporter protein B', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2438, 'cytochrome b6-f complex iron-sulfur subunit', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2457, 'cytochrome b', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2458, 'cytochrome c1, heme protein precursor', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2459, 'heat shock protein', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2460, 'O-antigen export system ATP-binding protein RFBE(rfbE)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2461, 'O-antigen export system permease RFBA (rfbA)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2462, 'NIFR3-like protein (nifR3)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2463, 'SCO2 protein precursor (sco2)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2464, 'protein disaggregation chaperone', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2465, 'cell cycle protein MESJ (mesJ)', NULL, 4);
INSERT INTO product (id, name, description, piece_type_id) VALUES (2466, 'cell division protein FTSH (ftsH)', NULL, 4);

SELECT pg_catalog.setval('product_id_seq', 2466, true);

INSERT INTO remote_db (id, name, description, url, nature_id) VALUES (1, 'NCBI', 'Нициональный центр биотехнологической информации', 'http://www.ncbi.nlm.nih.gov', 1);

SELECT pg_catalog.setval('remote_db_id_seq', 1, true);

SELECT pg_catalog.setval('tie_id_seq', 1, false);

INSERT INTO translator (id, name, description) VALUES (1, 'Google translate', 'http://translate.google.ru/');
INSERT INTO translator (id, name, description) VALUES (2, 'PROMT (translate.ru)', 'http://www.translate.ru/');
INSERT INTO translator (id, name, description) VALUES (3, 'InterTran', 'http://mrtranslate.ru/translators/intertran.html');

SELECT pg_catalog.setval('translator_id_seq', 3, true);

COMMIT;
--17.08.2014 23:45:20
