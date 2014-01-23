--24.01.2014 2:59:51
BEGIN;

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';

CREATE EXTENSION IF NOT EXISTS plv8 WITH SCHEMA pg_catalog;

COMMENT ON EXTENSION plv8 IS 'PL/JavaScript (v8) trusted procedural language';

CREATE FUNCTION check_element_in_alphabet(chain_id bigint, element_id bigint) RETURNS boolean
    LANGUAGE plv8
    AS $_$var plan = plv8.prepare( 'SELECT count(*) = 1 result FROM (SELECT unnest(alphabet) a FROM chain WHERE id = $1) c WHERE c.a = $2', ['bigint', 'bigint']);
var result = plan.execute([chain_id, element_id])[0].result;
plan.free();
return result; $_$;

COMMENT ON FUNCTION check_element_in_alphabet(chain_id bigint, element_id bigint) IS 'Функция для проверки наличия элемента с указанным id в алфавите указанной цепочки.';

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

CREATE FUNCTION seq_next_value(seq_name text) RETURNS bigint
    LANGUAGE plpgsql
    AS $$BEGIN
	return nextval(seq_name::regclass);
END;$$;

COMMENT ON FUNCTION seq_next_value(seq_name text) IS 'Функция возвращающая следующее значение указанной последовательности.';

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
  plv8.elog(ERROR, 'Нельзя добавлять запись в ', TG_TABLE_NAME, ' без предварительного добавления записи в таблицу chain или её потомок. id = ', NEW.id);
 }
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операции добавления записей в таблице с полем id.');
}$_$;

CREATE FUNCTION trigger_characteristics_link() RETURNS trigger
    LANGUAGE plv8
    AS $_$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "INSERT" || TG_OP == "UPDATE"){
	var linkOk = plv8.execute('SELECT linkability=($1 IS NOT NULL) AS result FROM characteristic_type WHERE id = $2;', [NEW.link_id, NEW.characteristic_type_id])[0].result;
	if(linkOk){
		return NEW;
	}
	else{
		plv8.elog(ERROR, 'Добавлемая характеристика имеет неверную привязку.');
	}
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей в таблице с полями characteristic_type_id и link_id');
}$_$;

COMMENT ON FUNCTION trigger_characteristics_link() IS 'Триггерная функция, проверяющая что у характеристики не задана привязка если она непривязываема, и задана любая привязка если она необходима.';

CREATE FUNCTION trigger_check_alphabet() RETURNS trigger
    LANGUAGE plv8
    AS $$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "INSERT" || TG_OP == "UPDATE"){
	var orphanedElements = plv8.execute('SELECT 1 result FROM (SELECT DISTINCT unnest(alphabet) a FROM chain) c LEFT OUTER JOIN element_key e ON e.id = c.a WHERE e.id IS NULL;').length;
	if(orphanedElements == 0){
		return true;
	} else{
		return plv8.elog(ERROR, 'В БД отсутствует ', orphanedElements, ' элементов алфавита.');
	}
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей в таблице с полем alphabet.');
}$$;

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
	var applicabilityOk = plv8.execute('SELECT ', ARGV[0], ' AS result FROM characteristic_type WHERE id = $1;', [NEW.characteristic_type_id])[0].result;
	if(applicabilityOk){
		return NEW;
	}
	else{
		plv8.elog(ERROR, 'Добавлемая характеристика неприменима к данному типу цепочки.');
	}
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей в таблице с полями characteristic_type_id.');
}$_$;

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
	var elementUsedCount = plv8.execute('SELECT count(*) result FROM (SELECT DISTINCT unnest(alphabet) a FROM chain) c WHERE c.a = $1)', [OLD.id])[0].result;
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
        IF (TG_OP = 'INSERT' OR TG_OP = 'UPDATE') THEN
            NEW.modified := now();
            RETURN NEW;
        END IF;
        RAISE EXCEPTION 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей в таблицах с полем modified.';
    END;
$$;

COMMENT ON FUNCTION trigger_set_modified() IS 'Триггерная функция, добавляющая текущее время и дату в поле modified.';

SET default_tablespace = '';

SET default_with_oids = false;

CREATE TABLE accidental (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    description character varying(255)
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
    description character varying(255),
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
    modified timestamp with time zone DEFAULT now() NOT NULL
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
    description character varying(255)
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
    description character varying(255),
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
    web_api_id integer
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

CREATE TABLE element_key (
    id bigint NOT NULL
);

COMMENT ON TABLE element_key IS 'Таблица, содаржащая id всех таблиц элементов.';

COMMENT ON COLUMN element_key.id IS 'Уникальный идентификатор.';

CREATE TABLE fmotiv (
    id bigint DEFAULT nextval('elements_id_seq'::regclass),
    notation_id integer DEFAULT 6,
    matter_id bigint DEFAULT 508,
    value character varying(255),
    description character varying(255),
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

COMMENT ON COLUMN fmotiv.value IS 'Ф-мотив в виде строки.';

COMMENT ON COLUMN fmotiv.description IS 'Описание Ф-мотива.';

COMMENT ON COLUMN fmotiv.name IS 'Название Ф-мотива.';

COMMENT ON COLUMN fmotiv.fmotiv_type_id IS 'Тип Ф-мотива.';

CREATE TABLE fmotiv_type (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    description character varying(255)
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
    description character varying(255)
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
    description character varying(255)
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
    language_id integer NOT NULL
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

CREATE TABLE matter (
    id bigint NOT NULL,
    name character varying(255) NOT NULL,
    nature_id integer NOT NULL,
    description character varying(255),
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
    value character varying(255),
    description character varying(255),
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

COMMENT ON COLUMN measure.value IS 'Такт в виде строки.';

COMMENT ON COLUMN measure.description IS 'Описание.';

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
    description character varying(255)
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
    description character varying(255),
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
    description character varying(255)
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

CREATE TABLE piece_type (
    id integer NOT NULL,
    name character varying(100),
    description character varying(255),
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

CREATE TABLE remote_db (
    id integer NOT NULL,
    name character varying(100),
    description character varying(255),
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
    description character varying(255)
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

ALTER TABLE ONLY piece_type ALTER COLUMN id SET DEFAULT nextval('piece_type_id_seq'::regclass);

ALTER TABLE ONLY pitch ALTER COLUMN id SET DEFAULT nextval('pitch_id_seq'::regclass);

ALTER TABLE ONLY remote_db ALTER COLUMN id SET DEFAULT nextval('remote_db_id_seq'::regclass);

ALTER TABLE ONLY tie ALTER COLUMN id SET DEFAULT nextval('tie_id_seq'::regclass);

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

ALTER TABLE ONLY piece_type
    ADD CONSTRAINT pk_piece_type PRIMARY KEY (id);

ALTER TABLE ONLY pitch
    ADD CONSTRAINT pk_pitch PRIMARY KEY (id);

ALTER TABLE ONLY remote_db
    ADD CONSTRAINT pk_remote_db PRIMARY KEY (id);

ALTER TABLE ONLY tie
    ADD CONSTRAINT pk_tie PRIMARY KEY (id);

ALTER TABLE ONLY accidental
    ADD CONSTRAINT uk_accidental_name UNIQUE (name);

ALTER TABLE ONLY binary_characteristic
    ADD CONSTRAINT uk_binary_characteristic_value UNIQUE (chain_id, characteristic_type_id, link_id, first_element_id, second_element_id);

ALTER TABLE ONLY characteristic_group
    ADD CONSTRAINT uk_characteristic_group_name UNIQUE (name);

ALTER TABLE ONLY characteristic_type
    ADD CONSTRAINT uk_characteristic_type_name UNIQUE (name);

ALTER TABLE ONLY characteristic
    ADD CONSTRAINT uk_characteristic_value UNIQUE (chain_id, characteristic_type_id, link_id);

ALTER TABLE ONLY congeneric_characteristic
    ADD CONSTRAINT uk_congeneric_characteristic UNIQUE (chain_id, characteristic_type_id, link_id, element_id);

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

ALTER TABLE ONLY piece_type
    ADD CONSTRAINT uk_piece_type UNIQUE (name);

ALTER TABLE ONLY remote_db
    ADD CONSTRAINT uk_remote_db_name UNIQUE (name);

ALTER TABLE ONLY remote_db
    ADD CONSTRAINT uk_remote_db_url UNIQUE (url);

ALTER TABLE ONLY tie
    ADD CONSTRAINT uk_tie_name UNIQUE (name);

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

CREATE INDEX ix_remote_db_id ON remote_db USING btree (id);

COMMENT ON INDEX ix_remote_db_id IS 'Индекс первичного ключа таблицы remote_db.';

CREATE INDEX ix_remote_db_name ON remote_db USING btree (name);

COMMENT ON INDEX ix_remote_db_name IS 'Индекс по имени удалённой базы данных.';

CREATE INDEX ix_tie_id ON tie USING btree (id);

CREATE INDEX ix_tie_name ON tie USING btree (name);

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

CREATE TRIGGER tgiu_binary_charaacteristic_link BEFORE INSERT OR UPDATE OF characteristic_type_id, link_id ON binary_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_characteristics_link();

COMMENT ON TRIGGER tgiu_binary_charaacteristic_link ON binary_characteristic IS 'Триггер, проверяющий правильность привязки.';

CREATE TRIGGER tgiu_binary_characteristic_applicability BEFORE INSERT OR UPDATE OF characteristic_type_id ON binary_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_check_applicability('binary_chain_applicable');

COMMENT ON TRIGGER tgiu_binary_characteristic_applicability ON binary_characteristic IS 'Триггер, проверяющий применима ли указанная характеристика к бинарным цепочкам.';

CREATE TRIGGER tgiu_binary_characteristic_modified BEFORE INSERT OR UPDATE ON binary_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_binary_characteristic_modified ON binary_characteristic IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_binary_chracteristic_elements_in_alphabet BEFORE INSERT OR UPDATE OF first_element_id, second_element_id, chain_id ON binary_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_check_elements_in_alphabet();

COMMENT ON TRIGGER tgiu_binary_chracteristic_elements_in_alphabet ON binary_characteristic IS 'Триггер, проверяющий что оба элемента связываемые коэффициентом зависимости присутствуют в алфавите данной цепочки.';

CREATE TRIGGER tgiu_chain_alphabet AFTER INSERT OR UPDATE OF alphabet ON chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_check_alphabet();

COMMENT ON TRIGGER tgiu_chain_alphabet ON chain IS 'Проверяет наличие всех элементов добавляемого или изменяемого алфавита в БД.';

CREATE TRIGGER tgiu_chain_modified BEFORE INSERT OR UPDATE ON chain FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_chain_modified ON chain IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_charaacteristic_link BEFORE INSERT OR UPDATE OF characteristic_type_id, link_id ON characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_characteristics_link();

COMMENT ON TRIGGER tgiu_charaacteristic_link ON characteristic IS 'Триггер, проверяющий правильность привязки.';

CREATE TRIGGER tgiu_characteristic_applicability BEFORE INSERT OR UPDATE OF characteristic_type_id ON characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_check_applicability('full_chain_applicable');

COMMENT ON TRIGGER tgiu_characteristic_applicability ON characteristic IS 'Триггер, проверяющий применима ли указанная характеристика к полным цепочкам.';

CREATE TRIGGER tgiu_characteristic_modified BEFORE INSERT OR UPDATE ON characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_characteristic_modified ON characteristic IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_congeneric_charaacteristic_link BEFORE INSERT OR UPDATE OF characteristic_type_id, link_id ON congeneric_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_characteristics_link();

COMMENT ON TRIGGER tgiu_congeneric_charaacteristic_link ON congeneric_characteristic IS 'Триггер, проверяющий правильность привязки.';

CREATE TRIGGER tgiu_congeneric_characteristic_applicability BEFORE INSERT OR UPDATE OF characteristic_type_id ON congeneric_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_check_applicability('congeneric_chain_applicable');

COMMENT ON TRIGGER tgiu_congeneric_characteristic_applicability ON congeneric_characteristic IS 'Триггер, проверяющий применима ли указанная характеристика к однородным цепочкам.';

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

CREATE TRIGGER tgu_literature_chain_characteristics AFTER UPDATE ON literature_chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_literature_chain_characteristics ON literature_chain IS 'Триггер удаляющий все характеристки при обновлении цепочки.';

CREATE TRIGGER tgu_measure_characteristics AFTER UPDATE ON measure FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_measure_characteristics ON measure IS 'Триггер удаляющий все характеристки при обновлении цепочки.';

CREATE TRIGGER tgu_music_chain_characteristics AFTER UPDATE ON music_chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_music_chain_characteristics ON music_chain IS 'Триггер удаляющий все характеристки при обновлении цепочки.';

ALTER TABLE ONLY binary_characteristic
    ADD CONSTRAINT fk_binary_characteristic_chain_key FOREIGN KEY (chain_id) REFERENCES chain_key(id) ON UPDATE CASCADE ON DELETE CASCADE;

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

ALTER TABLE ONLY binary_characteristic
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

ALTER TABLE ONLY remote_db
    ADD CONSTRAINT fk_remote_db_nature FOREIGN KEY (nature_id) REFERENCES nature(id);

INSERT INTO accidental (id, name, description) VALUES (1, '-2', 'Дубль-бемоль');
INSERT INTO accidental (id, name, description) VALUES (2, '-1', 'Бемоль');
INSERT INTO accidental (id, name, description) VALUES (3, '0', 'Бекар');
INSERT INTO accidental (id, name, description) VALUES (4, '1', 'Диез');
INSERT INTO accidental (id, name, description) VALUES (5, '2', 'Дубль-диез');

SELECT pg_catalog.setval('accidental_id_seq', 23, true);

SELECT pg_catalog.setval('characteristic_group_id_seq', 1, false);

INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES (1, 'Мощность алфавита', NULL, NULL, 'AlphabetPower', false, true, false, false);
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

SELECT pg_catalog.setval('characteristic_type_id_seq', 27, true);

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

SELECT pg_catalog.setval('piece_type_id_seq', 3, true);

INSERT INTO remote_db (id, name, description, url, nature_id) VALUES (1, 'NCBI', 'Нициональный центр биотехнологической информации', 'http://www.ncbi.nlm.nih.gov', 1);

SELECT pg_catalog.setval('remote_db_id_seq', 1, true);

SELECT pg_catalog.setval('tie_id_seq', 1, false);

COMMIT;
--24.01.2014 2:59:51
