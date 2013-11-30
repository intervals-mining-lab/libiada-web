-- В данном файле хранится история всех команд, 
-- изменяющих структуру начального дампа 
-- и справочных таблиц, до состояния текущего дампа.

BEGIN;

-- 24.11.2013

-- Алфавит и строй перенесены в таблицу chain и соответствующие ей наследники.
-- Все ключи, ссылающиеся на строй и алфавит были заменены check constraint'ами, либо триггерами.

ALTER TABLE chain  ADD COLUMN alphabet bigint[];
COMMENT ON COLUMN chain.alphabet IS 'Алфавит цепочки.';
COMMENT ON COLUMN dna_chain.alphabet IS 'Алфавит цепочки.';
COMMENT ON COLUMN literature_chain.alphabet IS 'Алфавит цепочки.';
COMMENT ON COLUMN music_chain.alphabet IS 'Алфавит цепочки.';
COMMENT ON COLUMN fmotiv.alphabet IS 'Алфавит цепочки.';
COMMENT ON COLUMN measure.alphabet IS 'Алфавит цепочки.';

UPDATE chain SET alphabet = (
    SELECT array_agg(a.elements) FROM (
        SELECT element_id elements FROM 
            alphabet 
            WHERE chain_id = chain.id 
            GROUP BY element_id, alphabet.number 
            ORDER BY number
        ) a 
    );
    
ALTER TABLE chain ALTER COLUMN alphabet SET NOT NULL;


    
ALTER TABLE chain  ADD COLUMN building integer[];
COMMENT ON COLUMN chain.building IS 'Строй цепочки.';
COMMENT ON COLUMN dna_chain.building IS 'Строй цепочки.';
COMMENT ON COLUMN literature_chain.building IS 'Строй цепочки.';
COMMENT ON COLUMN music_chain.building IS 'Строй цепочки.';
COMMENT ON COLUMN fmotiv.building IS 'Строй цепочки.';
COMMENT ON COLUMN measure.building IS 'Строй цепочки.';

UPDATE chain SET building = (
    SELECT array_agg(b.numbers) FROM (
        SELECT number numbers FROM 
            building 
            WHERE chain_id = chain.id 
            GROUP BY building.number, building.index 
            ORDER BY index
        ) b 
    );

ALTER TABLE chain ALTER COLUMN building SET NOT NULL;

 ALTER TABLE congeneric_characteristic DROP CONSTRAINT fk_congeneric_characteristic_alphabet_element;
 ALTER TABLE binary_characteristic DROP CONSTRAINT fk_binary_characteristic_alphabet_first_element;
 ALTER TABLE binary_characteristic DROP CONSTRAINT fk_binary_characteristic_alphabet_second_element;
 
 CREATE FUNCTION check_element_in_alphabet(IN chain_id bigint, IN element_id bigint) RETURNS boolean AS
$BODY$var plan = plv8.prepare( 'SELECT count(*) = 1 result FROM (SELECT unnest(alphabet) a FROM chain WHERE id = $1) c WHERE c.a = $2', ['bigint', 'bigint']);
var result = plan.execute([chain_id, element_id])[0].result;
plan.free();
return result; $BODY$
LANGUAGE plv8 VOLATILE NOT LEAKPROOF;

COMMENT ON FUNCTION public.check_element_in_alphabet(IN bigint, IN bigint) IS 'Функция для проверки наличия элемента с указанным id в алфавите указанной цепочки.';

CREATE FUNCTION trigger_check_element_in_alphabet() RETURNS trigger AS
$BODY$//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
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
}$BODY$
LANGUAGE plv8 VOLATILE NOT LEAKPROOF;

COMMENT ON FUNCTION public.trigger_check_element_in_alphabet() IS 'Триггерная функция, проверяющая что элемент для которого вычислена характеристика однородной цепи есть в алфавите указанной цепочки. По сути замена для внешнего ключа ссылающегося на алфавит.';

CREATE TRIGGER tgiu_congeneric_chracteristic_element_in_alphabet BEFORE INSERT OR UPDATE ON congeneric_characteristic FOR EACH ROW EXECUTE PROCEDURE public.trigger_check_element_in_alphabet();

COMMENT ON TRIGGER tgiu_congeneric_chracteristic_element_in_alphabet ON congeneric_characteristic IS 'Триггер, проверяющий что элемент однородной цепочки, для которой вычислена характеристика, присутствует в алфавите данной цепочки.';

CREATE FUNCTION trigger_check_elements_in_alphabet() RETURNS trigger AS
$BODY$//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
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
}$BODY$
LANGUAGE plv8 VOLATILE NOT LEAKPROOF;

COMMENT ON FUNCTION public.trigger_check_elements_in_alphabet() IS 'Триггерная функция, проверяющая что элементы для которых вычислен коэффициент зависимости присутствуют в алфавите указанной цепочки. По сути замена для внешних ключей ссылающихся на алфавит.';

CREATE TRIGGER tgiu_binary_chracteristic_elements_in_alphabet BEFORE INSERT OR UPDATE ON binary_characteristic FOR EACH ROW EXECUTE PROCEDURE public.trigger_check_elements_in_alphabet();

COMMENT ON TRIGGER tgiu_binary_chracteristic_elements_in_alphabet ON binary_characteristic IS 'Триггер, проверяющий что оба элемента связываемые коэффициентом зависимости присутствуют в алфавите данной цепочки.';


ALTER TABLE chain ADD CONSTRAINT chk_chain_building_starts_from_1 CHECK (building[0] = 1);

CREATE OR REPLACE FUNCTION check_building(arr integer[])
  RETURNS integer AS
$BODY$DECLARE
    element integer;
    max integer := 0;
BEGIN
    FOREACH element IN ARRAY arr
    LOOP
    IF element > max + 1 THEN
        RETURN -1;
    END IF;
    IF element = max + 1 THEN
        max := element;
    END IF;
    END LOOP;
    RETURN max;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION check_building(integer[])
  OWNER TO postgres;
  
COMMENT ON FUNCTION check_building(integer[]) IS 'Функция, проверяющая что все элементы строя увеличиваются не более чем на 1 от предыдущего максимального значения. Возвращает -1 в случае неправильного строя. Если ошибок нет, то возвращает максимальное значение в строе.';

ALTER TABLE chain ADD CONSTRAINT chk_chain_building_alphabet_length CHECK (check_building(building) = array_length(alphabet, 1));

CREATE OR REPLACE FUNCTION check_alphabet(arr bigint[]) RETURNS boolean AS
$BODY$DECLARE
    elementsExist boolean;
BEGIN
    SELECT count(*) = 0 INTO elementsExist
        FROM element_key e 
        FULL OUTER JOIN unnest($1) c 
        ON c = e.id 
        WHERE e.id IS NULL;
    RETURN elementsExist;
END;$BODY$
LANGUAGE plpgsql VOLATILE NOT LEAKPROOF
COST 100;

COMMENT ON FUNCTION check_alphabet(bigint[]) IS 'Функция, проверяющая что все элементы алфавита есть в таблице элементов.';

ALTER TABLE chain ADD CONSTRAINT chk_chain_alphabet_in_element CHECK (check_alphabet(alphabet));

CREATE FUNCTION trigger_element_alphabet_delete_bound() RETURNS trigger AS
$BODY$//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);
if (TG_OP == "DELETE"){
    var elementUsedInAlphabet = plv8.execute('SELECT count(*) > 0 result FROM (SELECT DISTINCT unnest(alphabet) a FROM chain) c WHERE c.a = $1)', [OLD.id])[0].result;
    if (elementUsedInAlphabet){
        plv8.elog(ERROR, 'Невозможно удалить элемент, он всё ещё используется в алфавите одной или нескольких цепочек. id = ', OLD.id);
    }
    plv8.execute( 'DELETE FROM element_key WHERE id = $1', [OLD.id] );
    return OLD;
} else{
    plv8.elog(ERROR, 'Неизвестная операция. данный триггер нработает только с опредацией удаления из таблиц, имеющих поле id. TG_OP = ', TG_OP);
}$BODY$
LANGUAGE plv8 VOLATILE NOT LEAKPROOF;

COMMENT ON FUNCTION public.trigger_element_alphabet_delete_bound() IS 'Триггерная функция, проверяющая что удаляемый элемент не встречается в алфавитах цепочек.';

CREATE FUNCTION trigger_delete_chain_characteristics() RETURNS trigger AS
$BODY$//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "DELETE" || TG_OP == "UPDATE"){
    plv8.execute('DELETE FROM characteristic WHERE chain_id = $1', [OLD.id]);
    plv8.execute('DELETE FROM binary_characteristic WHERE chain_id = $1', [OLD.id]);
    plv8.execute('DELETE FROM congeneric_characteristic WHERE chain_id = $1', [OLD.id]);
} else{
    plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операций удаления и изменения записей в таблице с полем id.');
}$BODY$
LANGUAGE plv8 VOLATILE NOT LEAKPROOF;

COMMENT ON FUNCTION public.trigger_delete_chain_characteristics() IS 'Триггерная функция, удаляющая все характеристики при удалении или изменении цепочки.';

CREATE TRIGGER tgud_chain_characteristic_delete BEFORE UPDATE OF alphabet, building OR DELETE ON chain FOR EACH ROW EXECUTE PROCEDURE public.trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgud_chain_characteristic_delete ON chain IS 'Триггер, удаляющий все характеристки данной цепочки при её обновлении или удалении.';

CREATE TRIGGER tgud_dna_chain_characteristic_delete BEFORE UPDATE OF alphabet, building OR DELETE ON dna_chain FOR EACH ROW EXECUTE PROCEDURE public.trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgud_dna_chain_characteristic_delete ON dna_chain IS 'Триггер, удаляющий все характеристки данной цепочки при её обновлении или удалении.';


CREATE TRIGGER tgud_literature_chain_characteristic_delete BEFORE UPDATE OF alphabet, building OR DELETE ON literature_chain FOR EACH ROW EXECUTE PROCEDURE public.trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgud_literature_chain_characteristic_delete ON literature_chain IS 'Триггер, удаляющий все характеристки данной цепочки при её обновлении или удалении.';

CREATE TRIGGER tgud_music_chain_characteristic_delete BEFORE UPDATE OF alphabet, building OR DELETE ON music_chain FOR EACH ROW EXECUTE PROCEDURE public.trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgud_music_chain_characteristic_delete ON music_chain IS 'Триггер, удаляющий все характеристки данной цепочки при её обновлении или удалении.';

CREATE TRIGGER tgud_fmotiv_characteristic_delete BEFORE UPDATE OF alphabet, building OR DELETE ON fmotiv FOR EACH ROW EXECUTE PROCEDURE public.trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgud_fmotiv_characteristic_delete ON fmotiv IS 'Триггер, удаляющий все характеристки данной цепочки при её обновлении или удалении.';

CREATE TRIGGER tgud_measure_characteristic_delete BEFORE UPDATE OF alphabet, building OR DELETE ON measure FOR EACH ROW EXECUTE PROCEDURE public.trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgud_measure_characteristic_delete ON measure IS 'Триггер, удаляющий все характеристки данной цепочки при её обновлении или удалении.';

DROP TABLE building;

DROP TABLE alphabet;



-- 25.11.2013

-- Добавлены функции для добавления записей в таблицы цепочек.

CREATE FUNCTION create_chain(IN id bigint, IN notation_id integer, IN matter_id bigint, IN piece_type_id integer, IN alphabet bigint[], IN building integer[], IN creation_date timestamp with time zone DEFAULT now(), IN piece_position integer DEFAULT 0, IN dissimilar boolean DEFAULT false) RETURNS void AS
$BODY$plv8.execute('INSERT INTO chain (id, notation_id, creation_date, matter_id, dissimilar, piece_type_id, piece_position, alphabet, building) VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9)',[id, notation_id, creation_date, matter_id, dissimilar, piece_type_id, piece_position, alphabet, building]);$BODY$
LANGUAGE plv8 VOLATILE NOT LEAKPROOF;
COMMENT ON FUNCTION public.create_chain(IN bigint, IN integer, IN bigint, IN integer, IN bigint[], IN integer[], IN timestamp with time zone, IN integer, IN boolean) IS 'Функция для создания записей в таблице chain.';

CREATE FUNCTION create_dna_chain(IN id bigint, IN notation_id integer, IN matter_id bigint, IN piece_type_id integer, IN fasta_header character varying, IN alphabet bigint[], IN building integer[], IN creation_date timestamp with time zone DEFAULT now(), IN piece_position integer DEFAULT 0, IN dissimilar boolean DEFAULT false) RETURNS void AS
$BODY$plv8.execute('INSERT INTO dna_chain (id, notation_id, creation_date, matter_id, dissimilar,	piece_type_id,	piece_position, fasta_header, alphabet, building) VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10)',[id, notation_id, creation_date, matter_id, dissimilar, piece_type_id, piece_position, fasta_header, alphabet, building]);$BODY$
LANGUAGE plv8 VOLATILE NOT LEAKPROOF;
COMMENT ON FUNCTION public.create_dna_chain(IN bigint, IN integer, IN bigint, IN integer, IN character varying, IN bigint[], IN integer[], IN timestamp with time zone, IN integer, IN boolean) IS 'Функция для создания записей в таблице dna_chain.';

CREATE FUNCTION create_literature_chain(IN id bigint, IN notation_id integer, IN matter_id bigint, IN piece_type_id integer, IN original boolean, IN language_id integer, IN alphabet bigint[], IN building integer[], IN creation_date timestamp with time zone DEFAULT now(), IN piece_position integer DEFAULT 0, IN dissimilar boolean DEFAULT false) RETURNS void AS
$BODY$plv8.execute('INSERT INTO literature_chain (id, notation_id, creation_date, matter_id, dissimilar, piece_type_id, piece_position, original, language_id, alphabet, building) VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10,$11)',[id, notation_id, creation_date, matter_id, dissimilar,	piece_type_id,	piece_position, original, language_id, alphabet, building]);$BODY$
LANGUAGE plv8 VOLATILE NOT LEAKPROOF;
COMMENT ON FUNCTION public.create_literature_chain(IN bigint, IN integer, IN bigint, IN integer, IN boolean, IN integer, IN bigint[], IN integer[], IN timestamp with time zone, IN integer, IN boolean) IS 'Функция для создания записей в таблице literature_chain.';

CREATE FUNCTION create_music_chain(IN id bigint, IN notation_id integer, IN matter_id bigint, IN piece_type_id integer, IN alphabet bigint[], IN building integer[], IN creation_date timestamp with time zone DEFAULT now(), IN piece_position integer DEFAULT 0, IN dissimilar boolean DEFAULT false) RETURNS void AS
$BODY$plv8.execute('INSERT INTO music_chain (id, notation_id, creation_date, matter_id, dissimilar, piece_type_id, piece_position, alphabet, building) VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9)',[id, notation_id, creation_date, matter_id, dissimilar,	piece_type_id,	piece_position, alphabet, building]);$BODY$
LANGUAGE plv8 VOLATILE NOT LEAKPROOF;
COMMENT ON FUNCTION public.create_music_chain(IN bigint, IN integer, IN bigint, IN integer, IN bigint[], IN integer[], IN timestamp with time zone, IN integer, IN boolean) IS 'Функция для создания записей в таблице music_chain.';

CREATE FUNCTION create_fmotiv(IN id bigint, IN piece_type_id integer, IN value character varying, IN description character varying, IN name character varying, IN fmotiv_type_id integer, IN alphabet bigint[], IN building integer[], IN creation_date timestamp with time zone DEFAULT now(), IN piece_position integer DEFAULT 0, IN dissimilar boolean DEFAULT false) RETURNS void AS
$BODY$plv8.execute('INSERT INTO fmotiv (id, notation_id, creation_date, matter_id, dissimilar, piece_type_id, piece_position, value, description, name, fmotiv_type_id, alphabet, building) VALUES ($1,6,$2,508,$3,$4,$5,$6,$7,$8,$9,$10,$11)',[id, creation_date, dissimilar, piece_type_id, piece_position, value, description, name, fmotiv_type_id, alphabet, building]);$BODY$
LANGUAGE plv8 VOLATILE NOT LEAKPROOF;
COMMENT ON FUNCTION public.create_fmotiv(IN bigint, IN integer, IN character varying, IN character varying, IN character varying, IN integer, IN bigint[], IN integer[], IN timestamp with time zone, IN integer, IN boolean) IS 'Функция для создания записей в таблице fmotiv.';

CREATE FUNCTION create_measure(IN id bigint, IN piece_type_id integer, IN value character varying, IN description character varying, IN name character varying, IN beats integer,IN beatbase integer,IN ticks_per_beat integer,IN fifths integer, IN major boolean, IN alphabet bigint[], IN building integer[], IN creation_date timestamp with time zone DEFAULT now(), IN piece_position integer DEFAULT 0, IN dissimilar boolean DEFAULT false) RETURNS void AS
$BODY$plv8.execute('INSERT INTO measure (id, notation_id, creation_date, matter_id, dissimilar, piece_type_id, piece_position, value, description, name, beats, beatbase, ticks_per_beat, fifths, major, alphabet, building) VALUES ($1,7,$2,509,$3,$4,$5,$6,$7,$8,$9,$10,$11,$12,$13,$14,$15)',[id, creation_date, dissimilar, piece_type_id, piece_position, value, description, name, beats, beatbase, ticks_per_beat, fifths, major, alphabet, building]);$BODY$
LANGUAGE plv8 VOLATILE NOT LEAKPROOF;
COMMENT ON FUNCTION public.create_measure(IN bigint, IN integer, IN character varying, IN character varying, IN character varying, IN integer, IN integer, IN integer, IN integer, IN boolean, IN bigint[], IN integer[], IN timestamp with time zone, IN integer, IN boolean) IS 'Функция для создания записей в таблице measure.';

ALTER TABLE fmotiv ALTER COLUMN notation_id SET DEFAULT 6;

ALTER TABLE measure ALTER COLUMN notation_id SET DEFAULT 7;



-- 26.11.2013

-- Удалены ненужные функции.

DROP FUNCTION create_alphabet_from_string(bigint, text);

DROP FUNCTION create_alphabet(bigint, bigint[]);

DROP FUNCTION create_building_from_string(bigint, text);

DROP FUNCTION create_building(bigint, integer[]);

DROP FUNCTION alphabet_count(bigint);

DROP FUNCTION building_count(bigint);



-- 27.11.2013

-- Переписана функция проверки целостности БД.

DROP FUNCTION db_integrity_test();

CREATE OR REPLACE FUNCTION db_integrity_test()
  RETURNS void AS
$BODY$
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

function CheckAlphabet() {
   //TODO: написать проверки того что все элементы алфавита есть в таблице element или её наследниках. 
   //TODO: Проверить что все бинарные и однородныее характеристики вычислены для элементов присутствующих в алфавите.
}

function db_integrity_test() {
    plv8.elog(INFO, "Процедура проверки целостности БД запущена");
    CheckChain();
    CheckAlphabet();
}

db_integrity_test();$BODY$
  LANGUAGE plv8 VOLATILE
  COST 100;

COMMENT ON FUNCTION db_integrity_test() IS 'Функция для проверки целостности данных в базе.';



-- 28.11.2013

-- Проверка алфавита в виде check constraint не позволяла создавать дампы 
-- (видимо проверка запускалась раньше чем заполнялась таблица alphabet).
-- Поэтому проверка заменена на триггер.

ALTER TABLE chain DROP CONSTRAINT chk_chain_alphabet_in_element;

DROP FUNCTION check_alphabet(bigint[]);

CREATE FUNCTION trigger_check_alphabet() RETURNS trigger AS
$BODY$//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "INSERT" || TG_OP == "UPDATE"){
    var plan = plv8.prepare('SELECT count(*) result FROM element_key e INNER JOIN unnest($1) c ON c = e.id;', ['bigint[]']);
    var existingElementsCount = plan.execute([NEW.alphabet])[0].result;
    if(existingElementsCount == NEW.alphabet.length){
        return NEW;
    } else{
        plv8.elog(ERROR, 'В БД отсутствует ', NEW.alphabet.length - existingElementsCount, 'Элементов алфавита добавляемой цепочки.');
    }
} else{
    plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей в таблице с полем alphabet.');
}$BODY$
LANGUAGE plv8 VOLATILE NOT LEAKPROOF;

COMMENT ON FUNCTION public.trigger_check_alphabet() IS 'Триггерная функция, проверяющая что все элементы алфавита добавляемой цепочки есть в базе.';

COMMIT;