BEGIN;

-- 31.09.2014
-- Добавлены и переименованы некоторые характеристики.

UPDATE characteristic_type SET class_name = 'AverageRemotenessDispersion' WHERE id = 28;
UPDATE characteristic_type SET class_name = 'AverageRemotenessStandardDeviation' WHERE id = 29;
UPDATE characteristic_type SET class_name = 'AverageRemotenessSkewness' WHERE id = 30;

INSERT INTO characteristic_type (name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES ('Нормированная ассиметрия средних удаленностей', 'коэффициент ассиметрии (скошенность) распределения средних удаленностей однородных цепей', NULL, 'NormalizedAverageRemotenessSkewness', true, true, false, false);
INSERT INTO characteristic_type (name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) VALUES ('Коэффициент соответствия', 'Коэффициент соответствия двух однородных цепей друг другу', NULL, 'ComplianceDegree', false, false, false, true);

-- 06.09.2014
-- Добавлены индексы на гены.

CREATE INDEX ix_piece_id ON piece (id ASC NULLS LAST);
CREATE INDEX ix_piece_gene_id ON piece (gene_id ASC NULLS LAST);

-- 05.10.2014
-- Добавлен новый тип РНК.

INSERT INTO piece_type (name, description, nature_id) VALUES ('Различная РНК', 'misc_RNA - miscellaneous other RNA', 1)

-- 24.12.2014
-- Updating db_integrity_test function.

DROP FUNCTION db_integrity_test();

CREATE OR REPLACE FUNCTION db_integrity_test()
  RETURNS void AS
$BODY$
function CheckChain() {
    plv8.elog(INFO, "Проверяем целостность таблицы chain и её потомков.");

    var chain = plv8.execute('SELECT id FROM chain');
    var chainDistinct = plv8.execute('SELECT DISTINCT id FROM chain');
    if (chain.length != chainDistinct.length) {
        plv8.elog(ERROR, 'id таблицы chain и/или дочерних таблиц не уникальны.');
    }else{
		plv8.elog(INFO, "id всех цепочек уникальны.");
    }
	
    plv8.elog(INFO, "Проверяем соответствие всех записей таблицы chain и её наследников с записями в таблице chain_key.");
    
    var chainDisproportion = plv8.execute('SELECT c.id, ck.id FROM (SELECT id FROM chain UNION SELECT id FROM gene) c FULL OUTER JOIN chain_key ck ON ck.id = c.id WHERE c.id IS NULL OR ck.id IS NULL');
    
    if (chainDisproportion.length > 0) {
		var debugQuery = 'SELECT c.id chain_id, ck.id chain_key_id FROM (SELECT id FROM chain UNION SELECT id FROM gene) c FULL OUTER JOIN chain_key ck ON ck.id = c.id WHERE c.id IS NULL OR ck.id IS NULL';
        plv8.elog(ERROR, 'Количество записей в таблице chain_key не совпадает с количеством записей с таблице chain и её наследниках. Для подробностей выполните "', debugQuery, '".');
    }else{
		plv8.elog(INFO, "Все записи в таблицах цепочек однозначно соответствуют записям в таблице chain_key.");
    }
	
	plv8.elog(INFO, 'Таблицы цепочек успешно проверены.');
}

function CheckElement() {
    plv8.elog(INFO, "Проверяем целостность таблицы element и её потомков.");

    var element = plv8.execute('SELECT id FROM element');
    var elementDistinct = plv8.execute('SELECT DISTINCT id FROM element');
    if (element.length != elementDistinct.length) {
        plv8.elog(ERROR, 'id таблицы element и/или дочерних таблиц не уникальны.');
    }else{
		plv8.elog(INFO, "id всех элементов уникальны.");
    }

    plv8.elog(INFO, "Проверяем соответствие всех записей таблицы element и её наследников с записями в таблице element_key.");
    
    var elementDisproportion = plv8.execute('SELECT c.id, ck.id FROM element c FULL OUTER JOIN element_key ck ON ck.id = c.id WHERE c.id IS NULL OR ck.id IS NULL');
    
    if (elementDisproportion.length > 0) {
		var debugQuery = 'SELECT c.id, ck.id FROM element c FULL OUTER JOIN element_key ck ON ck.id = c.id WHERE c.id IS NULL OR ck.id IS NULL';
        plv8.elog(ERROR, 'Количество записей в таблице element_key не совпадает с количеством записей с таблице element и её наследниках. Для подробностей выполните "', debugQuery,'"');
    }else{
		plv8.elog(INFO, "Все записи в таблицах элементов однозначно соответствуют записям в таблице element_key.");
    }
	
	plv8.elog(INFO, 'Таблицы элементов успешно проверены.');
}

function CheckAlphabet() {
	plv8.elog(INFO, 'Проверяем алфавиты всех цепочек.');
	
	var orphanedElements = plv8.execute('SELECT c.a FROM (SELECT DISTINCT unnest(alphabet) a FROM chain) c LEFT OUTER JOIN element_key e ON e.id = c.a WHERE e.id IS NULL');
	if (orphanedElements.length > 0) { 
		var debugQuery = 'SELECT c.a FROM (SELECT DISTINCT unnest(alphabet) a FROM chain) c LEFT OUTER JOIN element_key e ON e.id = c.a WHERE e.id IS NULL';
		plv8.elog(ERROR, 'В БД отсутствует ', orphanedElements,' элементов алфавита. Для подробностей выполните "', debugQuery,'".');
	}
	else {
		plv8.elog(INFO, 'Все элементы всех алфавитов присутствуют в таблице element_key.');
	}
	
	//TODO: Проверить что все бинарные и однородныее характеристики вычислены для элементов присутствующих в алфавите.
	plv8.elog(INFO, 'Все алфавиты цепочек успешно проверены.');
}

function db_integrity_test() {
    plv8.elog(INFO, "Процедура проверки целостности БД запущена.");
    CheckChain();
    CheckElement();
    CheckAlphabet();
    plv8.elog(INFO, "Проверка целостности успешно завершена.");
}

db_integrity_test();
$BODY$
  LANGUAGE plv8 VOLATILE;

COMMENT ON FUNCTION db_integrity_test() IS 'Функция для проверки целостности данных в базе.';

-- 26.12.2014
-- Deleted dissimilar column

ALTER TABLE chain DROP COLUMN dissimilar;

-- 05.01.2015
-- Changed none link id to 0.

UPDATE link set id = 0 WHERE id = 5;

-- 10.01.2015
-- Renamed complement into complementary.

ALTER TABLE dna_chain RENAME COLUMN complement TO complementary;
ALTER TABLE gene RENAME COLUMN complement TO complementary;

-- 05.01.2015
-- Added translator check to literature_chain.

ALTER TABLE literature_chain ADD CONSTRAINT chk_original_translator CHECK ((original AND translator_id IS NULL) OR NOT original);

-- 05.01.2015
-- Created table for measurement sequences.

CREATE TABLE data_chain
(
  id bigint NOT NULL DEFAULT nextval('elements_id_seq'::regclass), -- Уникальный внутренний идентификатор цепочки.
  notation_id integer NOT NULL, -- Форма записи цепочки в зависимости от элементов (буквы, слова, нуклеотиды, триплеты, etc).
  created timestamp with time zone NOT NULL DEFAULT now(), -- Дата создания цепочки.
  matter_id bigint NOT NULL, -- Ссылка на объект исследования.
  piece_type_id integer NOT NULL DEFAULT 1, -- Тип фрагмента.
  piece_position bigint NOT NULL DEFAULT 0, -- Позиция фрагмента.
  alphabet bigint[] NOT NULL, -- Алфавит цепочки.
  building integer[] NOT NULL, -- Строй цепочки.
  remote_id character varying(255), -- id цепочки в удалённой БД.
  remote_db_id integer, -- id удалённой базы данных, из которой взята данная цепочка.
  modified timestamp with time zone NOT NULL DEFAULT now(), -- Дата и время последнего изменения записи в таблице.
  description text, -- Описание отдельной цепочки.
  CONSTRAINT data_chain_pkey PRIMARY KEY (id),
  CONSTRAINT chk_remote_id CHECK (remote_db_id IS NULL AND remote_id IS NULL OR remote_db_id IS NOT NULL AND remote_id IS NOT NULL)
)
INHERITS (chain);

COMMENT ON TABLE data_chain IS 'Таблица массивов данных измерений.';
COMMENT ON COLUMN data_chain.id IS 'Уникальный внутренний идентификатор цепочки.';
COMMENT ON COLUMN data_chain.notation_id IS 'Форма записи цепочки в зависимости от элементов (буквы, слова, нуклеотиды, триплеты, etc).';
COMMENT ON COLUMN data_chain.created IS 'Дата создания цепочки.';
COMMENT ON COLUMN data_chain.matter_id IS 'Ссылка на объект исследования.';
COMMENT ON COLUMN data_chain.piece_type_id IS 'Тип фрагмента.';
COMMENT ON COLUMN data_chain.piece_position IS 'Позиция фрагмента.';
COMMENT ON COLUMN data_chain.alphabet IS 'Алфавит цепочки.';
COMMENT ON COLUMN data_chain.building IS 'Строй цепочки.';
COMMENT ON COLUMN data_chain.remote_id IS 'id цепочки в удалённой БД.';
COMMENT ON COLUMN data_chain.remote_db_id IS 'id удалённой базы данных, из которой взята данная цепочка.';
COMMENT ON COLUMN data_chain.modified IS 'Дата и время последнего изменения записи в таблице.';
COMMENT ON COLUMN data_chain.description IS 'Описание отдельной цепочки.';

CREATE INDEX data_chain_alphabet_idx ON data_chain USING gin (alphabet);

CREATE INDEX data_chain_matter_id_idx ON data_chain USING btree (matter_id);
COMMENT ON INDEX data_chain_matter_id_idx IS 'Индекс по объектам исследования которым принадлежат цепочки.';

CREATE INDEX data_chain_notation_id_idx ON data_chain USING btree (notation_id);
COMMENT ON INDEX data_chain_notation_id_idx IS 'Индекс по формам записи цепочек.';

CREATE INDEX data_chain_piece_type_id_idx ON data_chain USING btree (piece_type_id);
COMMENT ON INDEX data_chain_piece_type_id_idx IS 'Индекс по типам частей цепочек.';

ALTER TABLE data_chain ADD CONSTRAINT fk_data_chain_chain_key FOREIGN KEY (id) REFERENCES chain_key (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION DEFERRABLE INITIALLY DEFERRED;
ALTER TABLE data_chain ADD CONSTRAINT fk_data_chain_matter FOREIGN KEY (matter_id) REFERENCES matter (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE NO ACTION;
ALTER TABLE data_chain ADD CONSTRAINT fk_data_chain_notation FOREIGN KEY (notation_id) REFERENCES notation (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE NO ACTION;
ALTER TABLE data_chain ADD CONSTRAINT fk_data_chain_piece_type FOREIGN KEY (piece_type_id) REFERENCES piece_type (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE NO ACTION;
ALTER TABLE data_chain ADD CONSTRAINT fk_data_chain_remote_db FOREIGN KEY (remote_db_id) REFERENCES remote_db (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE NO ACTION;

CREATE TRIGGER tgi_data_chain_building_check BEFORE INSERT ON data_chain FOR EACH ROW EXECUTE PROCEDURE trigger_building_check();
COMMENT ON TRIGGER tgi_data_chain_building_check ON data_chain IS 'Триггер, проверяющий строй цепочки.';

CREATE TRIGGER tgiu_data_chain_alphabet AFTER INSERT OR UPDATE OF alphabet ON data_chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_check_alphabet();
COMMENT ON TRIGGER tgiu_data_chain_alphabet ON data_chain IS 'Проверяет наличие всех элементов добавляемого или изменяемого алфавита в БД.';

CREATE TRIGGER tgiu_data_chain_modified BEFORE INSERT OR UPDATE ON data_chain FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();
COMMENT ON TRIGGER tgiu_data_chain_modified ON data_chain IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiud_data_chain_chain_key_bound AFTER INSERT OR UPDATE OF id OR DELETE ON data_chain FOR EACH ROW EXECUTE PROCEDURE trigger_chain_key_bound();
COMMENT ON TRIGGER tgiud_data_chain_chain_key_bound ON data_chain IS 'Дублирует добавление, изменение и удаление записей в таблице chain в таблицу chain_key.';

CREATE TRIGGER tgu_data_chain_characteristics AFTER UPDATE ON data_chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_data_chain_characteristics ON data_chain IS 'Триггер удаляющий все характеристки при обновлении цепочки.';

-- 10.01.2015
-- Added new nature, notation and piece type for data chains.

INSERT INTO nature (id, name, description) VALUES (4, 'Measurement data sequences', 'Ordered arrays of measurement data');
INSERT INTO notation (id, name, description, nature_id) VALUES (10, 'Integer values', 'Numeric values of measured parameter', 4);
INSERT INTO piece_type (id, name, description, nature_id) VALUES (17, 'Complete numeric sequence', 'Full sequence of measured values', 4);

COMMIT;