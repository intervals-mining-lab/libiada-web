BEGIN;

-- 31.09.2014
-- Добавлены и переименованы некоторые характеристики.

UPDATE characteristic_type SET class_name = 'AverageRemotenessDispersion' WHERE id = 28;
UPDATE characteristic_type SET class_name = 'AverageRemotenessStandardDeviation' WHERE id = 29;
UPDATE characteristic_type SET class_name = 'AverageRemotenessSkewness' WHERE id = 30;

INSERT INTO characteristic_type (name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable)
 VALUES ('Нормированная ассиметрия средних удаленностей', 'коэффициент ассиметрии (скошенность) распределения средних удаленностей однородных цепей', NULL, 'NormalizedAverageRemotenessSkewness', true, true, false, false);

INSERT INTO characteristic_type (name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable)
 VALUES ('Коэффициент соответствия', 'Коэффициент соответствия двух однородных цепей друг другу', NULL, 'ComplianceDegree', false, false, false, true);

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

-- 13.01.2015
-- Added accordance characteristics table.

CREATE OR REPLACE FUNCTION trigger_check_elements_in_alphabets()
  RETURNS trigger AS
$BODY$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "INSERT" || TG_OP == "UPDATE"){
	var check_element_in_alphabet = plv8.find_function("check_element_in_alphabet");
	var firstElementInAlphabet = check_element_in_alphabet(NEW.first_chain_id, NEW.first_element_id);
	var secondElementInAlphabet = check_element_in_alphabet(NEW.second_chain_id, NEW.second_element_id);
	if(firstElementInAlphabet && secondElementInAlphabet){
		return NEW;
	}
	else if(firstElementInAlphabet){
		plv8.elog(ERROR, 'Добавлемая характеристика привязана к элементу, отсутствующему в алфавите цепочки. second_element_id = ', NEW.second_element_id,' ; chain_id = ', NEW.first_chain_id);
	} else{
		plv8.elog(ERROR, 'Добавлемая характеристика привязана к элементу, отсутствующему в алфавите цепочки. first_element_id = ', NEW.first_element_id,' ; chain_id = ', NEW.second_chain_id);
	}
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей в таблице с полями chain_id, first_element_id и second_element_id');
}$BODY$
  LANGUAGE plv8 VOLATILE
  COST 100;
COMMENT ON FUNCTION trigger_check_elements_in_alphabets() IS 'Триггерная функция, проверяющая что элементы для которых вычислен коэффициент соответствия присутствуют в алфавитах указанных цепочек. По сути замена для внешних ключей ссылающихся на алфавит.';

CREATE TABLE accordance_characteristic
(
  id bigserial NOT NULL, -- Уникальный внутренний идентификатор.
  first_chain_id bigint NOT NULL, -- Цепочка для которой вычислялась характеристика.
  second_chain_id bigint NOT NULL, -- Цепочка для которой вычислялась характеристика.
  characteristic_type_id integer NOT NULL, -- Вычисляемая характеристика.
  value double precision, -- Числовое значение характеристики.
  value_string text, -- Строковое значение характеристики.
  link_id integer, -- Привязка (если она используется).
  created timestamp with time zone NOT NULL DEFAULT now(), -- Дата вычисления характеристики.
  first_element_id bigint NOT NULL, -- id первого элемента из пары для которой вычисляется характеристика.
  second_element_id bigint NOT NULL, -- id второго элемента из пары для которой вычисляется характеристика.
  modified timestamp with time zone NOT NULL DEFAULT now(), -- Дата и время последнего изменения записи в таблице.
  CONSTRAINT pk_accordance_characteristic PRIMARY KEY (id),
  CONSTRAINT fk_accordance_characteristic_first_chain_key FOREIGN KEY (first_chain_id) REFERENCES chain_key (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_accordance_characteristic_second_chain_key FOREIGN KEY (second_chain_id) REFERENCES chain_key (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_accordance_characteristic_characteristic_type FOREIGN KEY (characteristic_type_id) REFERENCES characteristic_type (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE NO ACTION,
  CONSTRAINT fk_accordance_characteristic_element_key_first FOREIGN KEY (first_element_id) REFERENCES element_key (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE NO ACTION,
  CONSTRAINT fk_accordance_characteristic_element_key_second FOREIGN KEY (second_element_id) REFERENCES element_key (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE NO ACTION,
  CONSTRAINT fk_accordance_characteristic_link FOREIGN KEY (link_id) REFERENCES link (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE NO ACTION
);
ALTER TABLE accordance_characteristic OWNER TO postgres;
COMMENT ON TABLE accordance_characteristic IS 'Таблица со значениями характеристик зависимостей элементов.';
COMMENT ON COLUMN accordance_characteristic.id IS 'Уникальный внутренний идентификатор.';
COMMENT ON COLUMN accordance_characteristic.first_chain_id IS 'Первая цепочка для которой вычислялась характеристика.';
COMMENT ON COLUMN accordance_characteristic.second_chain_id IS 'Вторая цепочка для которой вычислялась характеристика.';
COMMENT ON COLUMN accordance_characteristic.characteristic_type_id IS 'Вычисляемая характеристика.';
COMMENT ON COLUMN accordance_characteristic.value IS 'Числовое значение характеристики.';
COMMENT ON COLUMN accordance_characteristic.value_string IS 'Строковое значение характеристики.';
COMMENT ON COLUMN accordance_characteristic.link_id IS 'Привязка (если она используется).';
COMMENT ON COLUMN accordance_characteristic.created IS 'Дата вычисления характеристики.';
COMMENT ON COLUMN accordance_characteristic.first_element_id IS 'id первого элемента из пары для которой вычисляется характеристика.';
COMMENT ON COLUMN accordance_characteristic.second_element_id IS 'id второго элемента из пары для которой вычисляется характеристика.';
COMMENT ON COLUMN accordance_characteristic.modified IS 'Дата и время последнего изменения записи в таблице.';

CREATE INDEX ix_accordance_characteristic_first_chain_id ON accordance_characteristic USING btree (first_chain_id);
COMMENT ON INDEX ix_accordance_characteristic_first_chain_id IS 'Индекс бинарных характеристик по цепочкам.';

CREATE INDEX ix_accordance_characteristic_second_chain_id ON accordance_characteristic USING btree (second_chain_id);
COMMENT ON INDEX ix_accordance_characteristic_second_chain_id IS 'Индекс бинарных характеристик по цепочкам.';

CREATE INDEX ix_accordance_characteristic_chain_link_characteristic_type ON accordance_characteristic USING btree (first_chain_id, second_chain_id, characteristic_type_id, link_id);
COMMENT ON INDEX ix_accordance_characteristic_chain_link_characteristic_type IS 'Индекс для выбора характеристики определённой цепочки с определённой привязкой.';

CREATE INDEX ix_accordance_characteristic_created ON accordance_characteristic USING btree (created);
COMMENT ON INDEX ix_accordance_characteristic_created IS 'Индекс характерисктик по датам их вычисления.';

CREATE UNIQUE INDEX uk_accordance_characteristic_value_link_not_null ON accordance_characteristic USING btree (first_chain_id, second_chain_id, characteristic_type_id, link_id, first_element_id, second_element_id) WHERE link_id IS NOT NULL;

CREATE UNIQUE INDEX uk_accordance_characteristic_value_link_null ON accordance_characteristic USING btree (first_chain_id, second_chain_id, characteristic_type_id, first_element_id, second_element_id) WHERE link_id IS NULL;

CREATE TRIGGER tgiu_accordance_characteristic_applicability BEFORE INSERT OR UPDATE OF characteristic_type_id ON accordance_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_check_applicability('binary_chain_applicable');
COMMENT ON TRIGGER tgiu_accordance_characteristic_applicability ON accordance_characteristic IS 'Триггер, проверяющий применима ли указанная характеристика к бинарным цепочкам.';

CREATE TRIGGER tgiu_accordance_characteristic_link BEFORE INSERT OR UPDATE OF characteristic_type_id, link_id ON accordance_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_characteristics_link();
COMMENT ON TRIGGER tgiu_accordance_characteristic_link ON accordance_characteristic IS 'Триггер, проверяющий правильность привязки.';

CREATE TRIGGER tgiu_accordance_characteristic_modified BEFORE INSERT OR UPDATE ON accordance_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();
COMMENT ON TRIGGER tgiu_accordance_characteristic_modified ON accordance_characteristic IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_binary_chracteristic_elements_in_alphabets BEFORE INSERT OR UPDATE OF first_chain_id, second_chain_id, first_element_id, second_element_id ON accordance_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_check_elements_in_alphabets();
COMMENT ON TRIGGER tgiu_binary_chracteristic_elements_in_alphabets ON accordance_characteristic IS 'Триггер, проверяющий что оба элемента связываемые коэффициентом зависимости присутствуют в алфавите данной цепочки.';

-- 14.01.2015 
-- Added statistical characteristics to characteristic_type

INSERT INTO characteristic_type (name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('GC ratio', '(G + C) / All * 100%', NULL, 'GCRatio', false, true, false, false);

INSERT INTO characteristic_type (name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('GC skew', '(G - C) / (G + C)', NULL, 'GCSkew', false, true, false, false);

INSERT INTO characteristic_type (name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('AT skew', '(A - T) / (A + T)', NULL, 'ATSkew', false, true, false, false);

INSERT INTO characteristic_type (name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('GC/AT ratio', '(G + C) / (A + T)', NULL, 'GCToATRatio', false, true, false, false);

INSERT INTO characteristic_type (name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('MK skew', '((C + A) - (G + T)) / All', NULL, 'MKSkew', false, true, false, false);

INSERT INTO characteristic_type (name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('RY skew', '((G + A) - (C + T)) / All', NULL, 'RYSkew', false, true, false, false);

INSERT INTO characteristic_type (name, description, characteristic_group_id, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('SW skew', '((G + C) - (A + T)) / All', NULL, 'SWSkew', false, true, false, false);
  
-- 16.01.2015
-- Added remoteness dispersion characteristics

INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('Average remoteness kurtosis', 'Average remoteness excess', 'AverageRemotenessKurtosis', true, true, false, false);
  
INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('Average remoteness kurtosis coefficient', 'Average remoteness excess coefficient', 'AverageRemotenessKurtosisCoefficient', true, true, false, false);

INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('Remoteness dispersion', NULL, 'RemotenessDispersion', true, true, false, false);
  
INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('Remoteness kurtosis', 'remoteness excess', 'RemotenessKurtosis', true, true, false, false);

INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('Remoteness kurtosis coefficient', 'Remoteness excess coefficient', 'RemotenessKurtosisCoefficient', true, true, false, false);

INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('Remoteness skewness', 'Remoteness assymetry', 'RemotenessSkewness', true, true, false, false);
  
INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('Remoteness skewness coefficient', 'Remoteness assymetry coefficient', 'RemotenessSkewnessCoefficient', true, true, false, false);
  
 INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('Remoteness standard deviation', NULL, 'RemotenessStandardDeviation', true, true, false, false);
  
UPDATE characteristic_type SET name = 'Average remoteness skewness coefficient', description = 'Average remoteness assymetry coefficient', class_name = 'AverageRemotenessSkewnessCoefficient' WHERE id = '31';
  
-- 20.01.2015
-- Added new column to characteristic type indicating if characteristic is accordance characteristic.
    
ALTER TABLE characteristic_type ADD COLUMN accordance_applicable boolean NOT NULL DEFAULT false;  

ALTER TABLE characteristic_type DROP CONSTRAINT chk_characteristic_applicable;

ALTER TABLE characteristic_type ADD CONSTRAINT chk_characteristic_applicable CHECK (full_chain_applicable OR binary_chain_applicable OR congeneric_chain_applicable OR accordance_applicable);
COMMENT ON CONSTRAINT chk_characteristic_applicable ON characteristic_type IS 'Проверяет что характеристика применима хотя бы к одному типу цепочек.';


INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable, accordance_applicable) 
  VALUES ('Compliance degree', NULL, 'ComplianceDegree', true, false, false, false, true);
  
-- 21.01.2015
-- Added entropy characteristics

INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('Entropy dispersion', NULL, 'EntropyDispersion', true, true, false, false);
  
INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('Entropy kurtosis', 'entropy excess', 'EntropyKurtosis', true, true, false, false);

INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('Entropy kurtosis coefficient', 'Entropy excess coefficient', 'EntropyKurtosisCoefficient', true, true, false, false);

INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('Entropy skewness', 'Entropy assymetry', 'EntropySkewness', true, true, false, false);
  
INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('Entropy skewness coefficient', 'Entropy assymetry coefficient', 'EntropySkewnessCoefficient', true, true, false, false);
  
 INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable) 
  VALUES ('Entropy standard deviation', NULL, 'EntropyStandardDeviation', true, true, false, false);

COMMIT;