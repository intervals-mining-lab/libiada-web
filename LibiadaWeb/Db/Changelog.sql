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

INSERT INTO piece_type (name, description, nature_id) VALUES ('Различная РНК', 'misc_RNA - miscellaneous other RNA', 1);

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
-- Added entropy characteristics.

INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable, accordance_applicable) 
  VALUES ('Entropy dispersion', NULL, 'EntropyDispersion', true, true, false, false, false);
  
INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable, accordance_applicable) 
  VALUES ('Entropy kurtosis', 'entropy excess', 'EntropyKurtosis', true, true, false, false, false);

INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable, accordance_applicable) 
  VALUES ('Entropy kurtosis coefficient', 'Entropy excess coefficient', 'EntropyKurtosisCoefficient', true, true, false, false, false);

INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable, accordance_applicable) 
  VALUES ('Entropy skewness', 'Entropy assymetry', 'EntropySkewness', true, true, false, false, false);
  
INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable, accordance_applicable) 
  VALUES ('Entropy skewness coefficient', 'Entropy assymetry coefficient', 'EntropySkewnessCoefficient', true, true, false, false, false);
  
INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable, accordance_applicable) 
  VALUES ('Entropy standard deviation', NULL, 'EntropyStandardDeviation', true, true, false, false, false);

-- 28.01.2015
-- New index on elements value.

CREATE INDEX ix_element_value ON element USING btree (value);
COMMENT ON INDEX ix_element_value IS 'Index in value of element.';

CREATE INDEX ix_element_value_notation ON element USING btree (value, notation_id);
COMMENT ON INDEX ix_element_value_notation IS 'Index on value and notation of element.';

-- 29.01.2015
-- Added forgotten characteristics types and deleted lost.

INSERT INTO characteristic_type (name, description, class_name, linkable, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable, accordance_applicable) 
  VALUES ('Average word length', 'Arithmetic mean of length of element in sequence', 'AverageWordLength', false, true, false, false, false);

DELETE FROM characteristic_type WHERE id = 19 AND class_name = 'BinaryGeometricMean';
DELETE FROM characteristic_type WHERE id = 32 AND class_name = 'ComplianceDegree';
DELETE FROM characteristic_type WHERE id = 13 AND class_name = 'ComplianceDegree';

UPDATE characteristic_type SET name = 'Elements count', class_name = 'ElementsCount', full_chain_applicable = true WHERE id = 4 AND class_name = 'Count';
UPDATE characteristic_type SET name = 'Cutting length', class_name = 'CuttingLength', congeneric_chain_applicable = true WHERE id = 5 AND class_name = 'CutLength';
UPDATE characteristic_type SET name = 'Cutting length vocabulary entropy', class_name = 'CuttingLengthVocabularyEntropy', congeneric_chain_applicable = true WHERE id = 6 AND class_name = 'CutLengthVocabularyEntropy';
UPDATE characteristic_type SET name = 'Geometric mean',  binary_chain_applicable = true WHERE id = 9 AND class_name = 'GeometricMean';
UPDATE characteristic_type SET name = 'Phantom messages count',  congeneric_chain_applicable = true WHERE id = 15 AND class_name = 'PhantomMessagesCount';
UPDATE characteristic_type SET name = 'Probability', description = 'Or frequency',  full_chain_applicable = true WHERE id = 15 AND class_name = 'Probability';

-- 23.02.2015
-- Refactoring of links.
-- Added new table containing characteristics types and links.

UPDATE link SET id = 5 WHERE id = 4;
UPDATE link SET id = 4 WHERE id = 3;
UPDATE link SET id = 3 WHERE id = 2;
UPDATE link SET id = 2 WHERE id = 1;
UPDATE link SET id = 1 WHERE id = 0;
INSERT INTO link (id, name, description) VALUES (0, 'Not applied', 'Link is not applied');

CREATE TABLE characteristic_type_link
(
   id serial NOT NULL, 
   characteristic_type_id integer NOT NULL, 
   link_id integer NOT NULL, 
   CONSTRAINT pk_characteristic_type_link PRIMARY KEY (id), 
   CONSTRAINT uk_characteristic_type_link UNIQUE (characteristic_type_id, link_id), 
   CONSTRAINT fk_characteristic_type_link_link FOREIGN KEY (link_id) REFERENCES link (id) ON UPDATE CASCADE ON DELETE CASCADE, 
   CONSTRAINT fk_characteristic_type_link_characteristic_type FOREIGN KEY (characteristic_type_id) REFERENCES characteristic_type (id) ON UPDATE CASCADE ON DELETE CASCADE
);
COMMENT ON TABLE characteristic_type_link IS 'Intermediate table of chracteristics types and their possible links.';

INSERT INTO characteristic_type_link (characteristic_type_id, link_id) (SELECT c.id, l.id FROM characteristic_type c INNER JOIN link l ON (c.linkable AND l.id != 0) OR (NOT c.linkable AND l.id = 0));

DELETE FROM characteristic;
DELETE FROM binary_characteristic;
DELETE FROM congeneric_characteristic;
DELETE FROM accordance_characteristic;

DROP TRIGGER tgiu_accordance_characteristic_applicability ON accordance_characteristic;
DROP TRIGGER tgiu_binary_characteristic_applicability ON binary_characteristic;
DROP TRIGGER tgiu_characteristic_applicability ON characteristic;
DROP TRIGGER tgiu_congeneric_characteristic_applicability ON congeneric_characteristic;

DROP TRIGGER tgiu_accordance_characteristic_link ON accordance_characteristic;
DROP TRIGGER tgiu_binary_characteristic_link ON binary_characteristic;
DROP TRIGGER tgiu_characteristic_link ON characteristic;
DROP TRIGGER tgiu_congeneric_characteristic_link ON congeneric_characteristic;

ALTER TABLE characteristic ADD COLUMN characteristic_type_link_id integer NOT NULL;
ALTER TABLE characteristic ADD CONSTRAINT fk_characteristic_type_link FOREIGN KEY (characteristic_type_link_id) REFERENCES characteristic_type_link (id) ON UPDATE CASCADE ON DELETE NO ACTION;
ALTER TABLE characteristic DROP CONSTRAINT fk_characteristic_link;
ALTER TABLE characteristic DROP CONSTRAINT fk_characteristic_type;
ALTER TABLE characteristic DROP COLUMN characteristic_type_id;
ALTER TABLE characteristic DROP COLUMN link_id;

ALTER TABLE binary_characteristic ADD COLUMN characteristic_type_link_id integer NOT NULL;
ALTER TABLE binary_characteristic ADD CONSTRAINT fk_characteristic_type_link FOREIGN KEY (characteristic_type_link_id) REFERENCES characteristic_type_link (id) ON UPDATE CASCADE ON DELETE NO ACTION;
ALTER TABLE binary_characteristic DROP CONSTRAINT fk_binary_characteristic_link;
ALTER TABLE binary_characteristic DROP CONSTRAINT fk_binary_characteristic_characteristic_type;
ALTER TABLE binary_characteristic DROP COLUMN characteristic_type_id;
ALTER TABLE binary_characteristic DROP COLUMN link_id;

ALTER TABLE congeneric_characteristic ADD COLUMN characteristic_type_link_id integer NOT NULL;
ALTER TABLE congeneric_characteristic ADD CONSTRAINT fk_characteristic_type_link FOREIGN KEY (characteristic_type_link_id) REFERENCES characteristic_type_link (id) ON UPDATE CASCADE ON DELETE NO ACTION;
ALTER TABLE congeneric_characteristic DROP CONSTRAINT fk_congeneric_characteristic_link;
ALTER TABLE congeneric_characteristic DROP CONSTRAINT fk_congeneric_characteristic_type;
ALTER TABLE congeneric_characteristic DROP COLUMN characteristic_type_id;
ALTER TABLE congeneric_characteristic DROP COLUMN link_id;

ALTER TABLE accordance_characteristic ADD COLUMN characteristic_type_link_id integer NOT NULL;
ALTER TABLE accordance_characteristic ADD CONSTRAINT fk_characteristic_type_link FOREIGN KEY (characteristic_type_link_id) REFERENCES characteristic_type_link (id) ON UPDATE CASCADE ON DELETE NO ACTION;
ALTER TABLE accordance_characteristic DROP CONSTRAINT fk_accordance_characteristic_link;
ALTER TABLE accordance_characteristic DROP CONSTRAINT fk_accordance_characteristic_characteristic_type;
ALTER TABLE accordance_characteristic DROP COLUMN characteristic_type_id;
ALTER TABLE accordance_characteristic DROP COLUMN link_id;

DROP FUNCTION trigger_check_applicability();

CREATE OR REPLACE FUNCTION trigger_check_applicability()
  RETURNS trigger AS
$BODY$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "NEW = ", JSON.stringify(NEW));
//plv8.elog(NOTICE, "OLD = ", JSON.stringify(OLD));
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "INSERT" || TG_OP == "UPDATE"){
	var applicabilityOk = plv8.execute('SELECT c.' + TG_ARGV[0] + ' AS result FROM characteristic_type_link cl INNER JOIN characteristic_type c ON c.id = cl.characteristic_type_id WHERE cl.id = $1;', [NEW.characteristic_type_link_id])[0].result;
	if(applicabilityOk){
		return NEW;
	}
	else{
		plv8.elog(ERROR, 'Добавлемая характеристика неприменима к данному типу цепочки.');
	}
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей в таблице с полями characteristic_type_id.');
}
$BODY$
  LANGUAGE plv8 VOLATILE
  COST 100;
COMMENT ON FUNCTION trigger_check_applicability() IS 'Триггерная функция, проверяющая, что характеристика может быть вычислена для такого типа цепочки';

CREATE TRIGGER tgiu_congeneric_characteristic_applicability BEFORE INSERT OR UPDATE OF characteristic_type_link_id ON congeneric_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_check_applicability('congeneric_chain_applicable');
COMMENT ON TRIGGER tgiu_congeneric_characteristic_applicability ON congeneric_characteristic IS 'Триггер, проверяющий применима ли указанная характеристика к однородным цепочкам.';

CREATE TRIGGER tgiu_characteristic_applicability BEFORE INSERT OR UPDATE OF characteristic_type_link_id ON characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_check_applicability('full_chain_applicable');
COMMENT ON TRIGGER tgiu_characteristic_applicability ON characteristic IS 'Триггер, проверяющий применима ли указанная характеристика к полным цепочкам.';

CREATE TRIGGER tgiu_binary_characteristic_applicability BEFORE INSERT OR UPDATE OF characteristic_type_link_id ON binary_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_check_applicability('binary_chain_applicable');
COMMENT ON TRIGGER tgiu_binary_characteristic_applicability ON binary_characteristic IS 'Триггер, проверяющий применима ли указанная характеристика к бинарным цепочкам.';

CREATE TRIGGER tgiu_accordance_characteristic_applicability BEFORE INSERT OR UPDATE OF characteristic_type_link_id ON accordance_characteristic FOR EACH ROW EXECUTE PROCEDURE trigger_check_applicability('accordance_applicable');
COMMENT ON TRIGGER tgiu_accordance_characteristic_applicability ON accordance_characteristic IS 'Триггер, проверяющий применима ли указанная характеристика к бинарным цепочкам.';

ALTER TABLE characteristic_type DROP COLUMN linkable;
DROP FUNCTION trigger_characteristics_link();

-- 10.03.2015
-- New tables for genes and other fragments of sequences.

ALTER TABLE piece_type RENAME TO feature;
ALTER TABLE chain RENAME COLUMN piece_type_id TO feature_id;
ALTER TABLE piece RENAME TO position;
ALTER TABLE position RENAME COLUMN gene_id TO fragment_id;

CREATE TABLE fragment
(
   id bigint NOT NULL DEFAULT nextval('elements_id_seq'::regclass), 
   created timestamp with time zone NOT NULL DEFAULT now(), 
   modified timestamp with time zone NOT NULL DEFAULT now(), 
   chain_id bigint NOT NULL, 
   start integer NOT NULL,
   length integer NOT NULL,
   feature_id integer NOT NULL, 
   web_api_id integer NOT NULL, 
   complementary boolean NOT NULL DEFAULT false, 
   partial boolean NOT NULL DEFAULT false, 
   CONSTRAINT pk_fragment PRIMARY KEY (id), 
   CONSTRAINT fk_fragment_chain_key FOREIGN KEY (id) REFERENCES chain_key (id) ON UPDATE NO ACTION ON DELETE NO ACTION DEFERRABLE INITIALLY DEFERRED, 
   CONSTRAINT fk_fragment_chain_chain_key FOREIGN KEY (chain_id) REFERENCES chain_key (id) ON UPDATE CASCADE ON DELETE CASCADE, 
   CONSTRAINT fk_fragment_feature FOREIGN KEY (feature_id) REFERENCES feature (id) ON UPDATE NO ACTION ON DELETE NO ACTION
);

CREATE TRIGGER tgiu_fragment_modified BEFORE INSERT OR UPDATE ON fragment FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();
COMMENT ON TRIGGER tgiu_fragment_modified ON fragment IS 'Trigger adding creation and modification dates.';

CREATE TRIGGER tgiud_fragment_chain_key_bound AFTER INSERT OR UPDATE OF id OR DELETE ON fragment FOR EACH ROW EXECUTE PROCEDURE trigger_chain_key_bound();
COMMENT ON TRIGGER tgiud_fragment_chain_key_bound ON fragment IS 'Creates two way bound with chain_key table.';

CREATE TRIGGER tgu_fragment_characteristics AFTER UPDATE ON fragment FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_fragment_characteristics ON fragment IS 'Deletes all calculated characteristics is sequence changes.';

ALTER TABLE position DROP CONSTRAINT fk_piece_gene;

ALTER TABLE position ADD CONSTRAINT fk_position_fragment FOREIGN KEY (fragment_id) REFERENCES fragment (id) ON UPDATE CASCADE ON DELETE CASCADE;

CREATE INDEX ix_fragment_chain_id ON fragment USING btree (chain_id);
CREATE INDEX ix_fragment_chain_feature ON fragment (chain_id, feature_id);

CREATE TABLE attribute
(
   id serial NOT NULL, 
   name text NOT NULL, 
   CONSTRAINT pk_attribute PRIMARY KEY (id), 
   CONSTRAINT uk_attribute UNIQUE (name)
);

CREATE TABLE chain_attribute
(
	id bigserial NOT NULL, 
   chain_id bigint NOT NULL, 
   attribute_id integer NOT NULL, 
   value text NOT NULL, 
   CONSTRAINT pk_chain_attribute PRIMARY KEY (id), 
   CONSTRAINT fk_chain_attribute_chain FOREIGN KEY (chain_id) REFERENCES chain_key (id) ON UPDATE CASCADE ON DELETE CASCADE, 
   CONSTRAINT fk_chain_attribute_attribute FOREIGN KEY (attribute_id) REFERENCES attribute (id) ON UPDATE CASCADE ON DELETE NO ACTION, 
   CONSTRAINT uk_chain_attribute UNIQUE (chain_id, attribute_id)
);

DROP TABLE gene;
ALTER TABLE dna_chain DROP COLUMN product_id;
DROP TABLE product;

-- 11.03.2015
-- Some fixes in fragments storage structure.

ALTER TABLE fragment ALTER COLUMN web_api_id DROP NOT NULL;
ALTER TABLE attribute ALTER COLUMN name SET DATA TYPE character varying(255);
ALTER TABLE feature ADD COLUMN complete boolean NOT NULL DEFAULT false;
ALTER TABLE feature ADD COLUMN type character varying(255);

CREATE INDEX ix_feature_type ON feature (type ASC NULLS LAST);

-- 12.03.2015
-- Adding dictionary data for fragments.

SELECT nextval('piece_type_id_seq');

UPDATE feature SET name = 'Complete gemone', complete = true WHERE id = 1;
UPDATE feature SET name = 'Complete text', complete = true WHERE id = 2;
UPDATE feature SET name = 'Complete piece of music', complete = true WHERE id = 3;
UPDATE feature SET name = 'Coding DNA sequence', type = 'CDS', description = 'Coding sequence; sequence of nucleotides that corresponds with the sequence of amino acids in a protein (location includes stop codon); feature includes amino acid conceptual translation.' WHERE id = 4;
UPDATE feature SET name = 'Ribosomal RNA', type = 'rRNA', description = 'RNA component of the ribonucleoprotein particle (ribosome) which assembles amino acids into proteins.' WHERE id = 5;
UPDATE feature SET name = 'Transfer RNA', type = 'tRNA', description = 'A small RNA molecule (75-85 bases long) that mediates the translation of a nucleic acid sequence into an amino acid sequence.' WHERE id = 6;
UPDATE feature SET name = 'Non-coding RNA', type = 'ncRNA', description = 'A non-protein-coding gene, other than ribosomal RNA and transfer RNA, the functional molecule of which is the RNA transcript' WHERE id = 7;
UPDATE feature SET name = 'Transfer-messenger RNA', type = 'tmRNA', description = 'tmRNA acts as a tRNA first, and then as an mRNA that encodes a peptide tag; the ribosome translates this mRNA region of tmRNA and attaches the encoded peptide tag to the C-terminus of the unfinished protein; this attached tag targets the protein for destruction or proteolysis' WHERE id = 8;
UPDATE feature SET name = 'Pseudo gene', type = 'pseudo' WHERE id = 9;
UPDATE feature SET name = 'Plasmid', complete = true WHERE id = 10;
UPDATE feature SET name = 'Mitochondrion genome', complete = true WHERE id = 11;
UPDATE feature SET name = 'Mitochondrion ribosomal RNA' WHERE id = 12;
UPDATE feature SET name = 'Repeat region', type = 'repeat_region', description = 'Region of genome containing repeating units.' WHERE id = 13;
UPDATE feature SET name = 'Non-coding sequence' WHERE id = 14;
UPDATE feature SET name = 'Chloroplast genome', complete = true WHERE id = 15;
UPDATE feature SET name = 'Miscellaneous other RNA', type = 'misc_RNA', description = 'Any transcript or RNA product that cannot be defined by other RNA keys (prim_transcript, precursor_RNA, mRNA, 5UTR, 3UTR, exon, CDS, sig_peptide, transit_peptide, mat_peptide, intron, polyA_site, ncRNA, rRNA and tRNA)' WHERE id = 16;
UPDATE feature SET complete = true WHERE id = 17;

INSERT INTO feature (name, description, nature_id, type) VALUES ('Miscellaneous feature', 'Region of biological interest which cannot be described by any other feature key; a new or rare feature.', 1, 'misc_feature');
INSERT INTO feature (name, description, nature_id, type) VALUES ('Messenger RNA', 'Includes 5untranslated region (5UTR), coding sequences (CDS, exon) and 3untranslated region (3UTR).', 1, 'mRNA');
INSERT INTO feature (name, description, nature_id, type) VALUES ('Regulatory', 'Any region of sequence that functions in the regulation of transcription or translation.', 1, 'regulatory');

-- 13.05.2015
-- New fragment unique key

ALTER TABLE fragment ADD CONSTRAINT uk_fragment UNIQUE (chain_id, start);

-- 14.05.2015
--Updating trigger functions and renaming tables.

ALTER TABLE fragment RENAME TO subsequence;
ALTER TABLE position RENAME COLUMN fragment_id TO subsequence_id;
ALTER TABLE subsequence RENAME CONSTRAINT pk_fragment TO pk_subsequence;
ALTER TABLE subsequence RENAME CONSTRAINT fk_fragment_chain_chain_key TO fk_subsequence_chain_chain_key;
ALTER TABLE subsequence RENAME CONSTRAINT fk_fragment_chain_key TO fk_subsequence_chain_key;
ALTER TABLE subsequence RENAME CONSTRAINT fk_fragment_feature TO fk_subsequence_feature;
ALTER TABLE subsequence RENAME CONSTRAINT uk_fragment TO uk_subsequence;
ALTER TABLE position RENAME CONSTRAINT fk_position_fragment TO fk_position_subsequence;
ALTER INDEX ix_fragment_chain_feature RENAME TO ix_subsequence_chain_feature;
ALTER INDEX ix_fragment_chain_id RENAME TO ix_subsequence_chain_id;
ALTER INDEX ix_piece_gene_id RENAME TO ix_position_subsequence_id;
ALTER INDEX ix_piece_id RENAME TO ix_position_id;

DROP TRIGGER tgiu_fragment_modified ON subsequence;
CREATE TRIGGER tgiu_subsequence_modified BEFORE INSERT OR UPDATE ON subsequence FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();
COMMENT ON TRIGGER tgiu_subsequence_modified ON subsequence IS 'Trigger adding creation and modification dates.';

DROP TRIGGER tgiud_fragment_chain_key_bound ON subsequence;
CREATE TRIGGER tgiud_subsequence_chain_key_bound AFTER INSERT OR UPDATE OF id OR DELETE ON subsequence FOR EACH ROW EXECUTE PROCEDURE trigger_chain_key_bound();
COMMENT ON TRIGGER tgiud_subsequence_chain_key_bound ON subsequence IS 'Creates two way bound with chain_key table.';

DROP TRIGGER tgu_fragment_characteristics ON subsequence;
CREATE TRIGGER tgu_subsequence_characteristics AFTER UPDATE ON subsequence FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_subsequence_characteristics ON subsequence IS 'Deletes all calculated characteristics is sequence changes.';

DROP FUNCTION check_genes_import_positions(bigint);

CREATE OR REPLACE FUNCTION trigger_chain_key_insert()
  RETURNS trigger AS
$BODY$
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
  var subsequence = plv8.execute('SELECT count(*) = 1 result FROM subsequence WHERE id = $1', [NEW.id])[0].result;
  if (subsequence){
   return NEW;
  }else{
   plv8.elog(ERROR, 'New record in table ', TG_TABLE_NAME, ' cannot be addded witout adding record with id=', NEW.id, ' in table sequence or its child.');
  }
 }
} else{
	plv8.elog(ERROR, 'Unknown db operation. This trigger only operates on INSET and UPDARE operations on tables with id column.');
}
$BODY$
  LANGUAGE plv8 VOLATILE
  COST 100;

CREATE OR REPLACE FUNCTION db_integrity_test()
  RETURNS void AS
$BODY$
function CheckChain() {
    plv8.elog(INFO, "Checking table sequence and its children.");

    var chain = plv8.execute('SELECT id FROM chain');
    var chainDistinct = plv8.execute('SELECT DISTINCT id FROM chain');
    if (chain.length != chainDistinct.length) {
        plv8.elog(ERROR, 'ids in table sequence and/or its cildren are not unique.');
    }else{
		plv8.elog(INFO, "All sequence ids are unique.");
    }
	
    plv8.elog(INFO, "Checking accordance of records in table sequence (and its children) to records in sequence_key table.");
    
    var chainDisproportion = plv8.execute('SELECT c.id, ck.id FROM (SELECT id FROM chain UNION SELECT id FROM subsequence) c FULL OUTER JOIN chain_key ck ON ck.id = c.id WHERE c.id IS NULL OR ck.id IS NULL');
    
    if (chainDisproportion.length > 0) {
		var debugQuery = 'SELECT c.id chain_id, ck.id chain_key_id FROM (SELECT id FROM chain UNION SELECT id FROM subsequence) c FULL OUTER JOIN chain_key ck ON ck.id = c.id WHERE c.id IS NULL OR ck.id IS NULL';
        plv8.elog(ERROR, 'Number of records in sequence_key is not equal to number of records in sequence and its children. For detail see "', debugQuery, '".');
    }else{
		plv8.elog(INFO, "sequence_key is in sync with sequence and its children.");
    }
	
	plv8.elog(INFO, 'Sequences tables are all checked.');
}

function CheckElement() {
    plv8.elog(INFO, "Checking table element and its children.");

    var element = plv8.execute('SELECT id FROM element');
    var elementDistinct = plv8.execute('SELECT DISTINCT id FROM element');
    if (element.length != elementDistinct.length) {
        plv8.elog(ERROR, 'ids in table element and/or its cildren are not unique.');
    }else{
		plv8.elog(INFO, "All element ids are unique.");
    }

    plv8.elog(INFO, "Checking accordance of records in table element (and its children) to records in element_key table.");
    
    var elementDisproportion = plv8.execute('SELECT c.id, ck.id FROM element c FULL OUTER JOIN element_key ck ON ck.id = c.id WHERE c.id IS NULL OR ck.id IS NULL');
    
    if (elementDisproportion.length > 0) {
		var debugQuery = 'SELECT c.id, ck.id FROM element c FULL OUTER JOIN element_key ck ON ck.id = c.id WHERE c.id IS NULL OR ck.id IS NULL';
        plv8.elog(ERROR, 'Number of records in element_key is not equal to number of records in element and its children. For detail see "', debugQuery,'"');
    }else{
		plv8.elog(INFO, "element_key is in sync with element and its children.");
    }
	
	plv8.elog(INFO, 'Elements tables are all checked.');
}

function CheckAlphabet() {
	plv8.elog(INFO, 'Checking alphabets of all sequences.');
	
	var orphanedElements = plv8.execute('SELECT c.a FROM (SELECT DISTINCT unnest(alphabet) a FROM chain) c LEFT OUTER JOIN element_key e ON e.id = c.a WHERE e.id IS NULL');
	if (orphanedElements.length > 0) { 
		var debugQuery = 'SELECT c.a FROM (SELECT DISTINCT unnest(alphabet) a FROM chain) c LEFT OUTER JOIN element_key e ON e.id = c.a WHERE e.id IS NULL';
		plv8.elog(ERROR, 'There are ', orphanedElements.length,' missing elements of alphabet. For details see "', debugQuery,'".');
	}
	else {
		plv8.elog(INFO, 'All alphabets elements are present in element_key table.');
	}
	
	//TODO: Проверить что все бинарные и однородные характеристики вычислены для элементов присутствующих в алфавите.
	plv8.elog(INFO, 'All alphabets are checked.');
}

function db_integrity_test() {
    plv8.elog(INFO, "Checking referential integrity of database.");
    CheckChain();
    CheckElement();
    CheckAlphabet();
    plv8.elog(INFO, "Referential integrity of database is successfully checked.");
}

db_integrity_test();
$BODY$
  LANGUAGE plv8 VOLATILE
  COST 100;
COMMENT ON FUNCTION db_integrity_test() IS 'Procedure for cheking referential integrity of db.';

-- 19.03.2015
-- New function for adding characteristic_type.

CREATE FUNCTION create_chatacteristic_type(IN name character varying, IN description text, IN characteristic_group_id integer, IN class_name character varying, IN full_sequence_applicable boolean, IN congeneric_sequence_applicable boolean, IN binary_sequence_applicable boolean, IN accordance_applicable boolean, IN linkable boolean) RETURNS integer AS
$BODY$
DECLARE
	id integer;
BEGIN
	SELECT nextval('characteristic_type_id_seq') INTO id;
	INSERT INTO characteristic_type (id, name, description, characteristic_group_id, class_name, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable, accordance_applicable) VALUES (id, name, description, characteristic_group_id, class_name, full_sequence_applicable, congeneric_sequence_applicable, binary_sequence_applicable, accordance_applicable);
	IF linkable THEN
		INSERT INTO characteristic_type_link (characteristic_type_id, link_id) (SELECT id, c.linkid FROM (SELECT link.id linkid FROM link WHERE link.id !=0) c);
	ELSE
		INSERT INTO characteristic_type_link (characteristic_type_id, link_id) VALUES (id, 0);
	END IF;
	RETURN id;
END;$BODY$
LANGUAGE plpgsql VOLATILE NOT LEAKPROOF;

COMMENT ON FUNCTION create_chatacteristic_type(IN character varying, IN text, IN integer, IN character varying, IN boolean, IN boolean, IN boolean, IN boolean, IN boolean) IS 'Function for adding characteristic_type and connected records to characteristic_type_link';

-- 27.03.2015
-- Added remoteness characteristics.
SELECT create_chatacteristic_type('Average remoteness AT skew', '(A - T) / (A + T)', NULL, 'AverageRemotenessATSkew', true, false, false, false, true);

SELECT create_chatacteristic_type('Average remoteness GC ratio', '(G + C) / All * 100%', NULL, 'AverageRemotenessGCRatio', true, false, false, false, true);

SELECT create_chatacteristic_type('Average remoteness GC skew', '(G - C) / (G + C)', NULL, 'AverageRemotenessGCSkew', true, false, false, false, true);

SELECT create_chatacteristic_type('Average remoteness GC/AT ratio', '(G + C) / (A + T)', NULL, 'AverageRemotenessGCToATRatio', true, false, false, false, true);

SELECT create_chatacteristic_type('Average remoteness MK skew', '((C + A) - (G + T)) / All', NULL, 'AverageRemotenessMKSkew', true, false, false, false, true);

SELECT create_chatacteristic_type('Average remoteness RY skew', '((G + A) - (C + T)) / All', NULL, 'AverageRemotenessRYSkew', true, false, false, false, true);

SELECT create_chatacteristic_type('Average remoteness SW skew', '((G + C) - (A + T)) / All', NULL, 'AverageRemotenessSWSkew', true, false, false, false, true);

-- 27.03.2015
-- Added attributes.

INSERT INTO attribute(name) VALUES ('db_xref');
INSERT INTO attribute(name) VALUES ('protein_id');
INSERT INTO attribute(name) VALUES ('complement');
INSERT INTO attribute(name) VALUES ('complement_join');
INSERT INTO attribute(name) VALUES ('product');
INSERT INTO attribute(name) VALUES ('note');
INSERT INTO attribute(name) VALUES ('codon_start');
INSERT INTO attribute(name) VALUES ('transl_table');
INSERT INTO attribute(name) VALUES ('inference');
INSERT INTO attribute(name) VALUES ('rpt_type');
INSERT INTO attribute(name) VALUES ('locus_tag');

-- 29.03.2015
-- Added more attributes.

INSERT INTO attribute(name) VALUES ('old_locus_tag');
INSERT INTO attribute(name) VALUES ('gene');
INSERT INTO attribute(name) VALUES ('anticodon');
INSERT INTO attribute(name) VALUES ('EC_number');
INSERT INTO attribute(name) VALUES ('exception');
INSERT INTO attribute(name) VALUES ('gene_synonym');
INSERT INTO attribute(name) VALUES ('pseudo');
INSERT INTO attribute(name) VALUES ('ncRNA_class');

-- 31.03.2015
-- Updated updated and created function;

CREATE OR REPLACE FUNCTION trigger_set_modified() RETURNS trigger AS
$BODY$
    BEGIN
	NEW.modified := now();
	IF (TG_OP = 'INSERT') THEN
            NEW.created := now();
	    RETURN NEW;
        END IF;
        IF (TG_OP = 'UPDATE') THEN
	    NEW.created = OLD.created;
            RETURN NEW;
        END IF;
        RAISE EXCEPTION 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей в таблицах с полями modified и created.';
    END;
$BODY$
LANGUAGE plpgsql VOLATILE NOT LEAKPROOF
COST 100;

-- 01.04.2015
-- Added new attributes.

INSERT INTO attribute(name) VALUES ('standard_name');
INSERT INTO attribute(name) VALUES ('rpt_family');
INSERT INTO attribute(name) VALUES ('direction');
INSERT INTO attribute(name) VALUES ('ribosomal_slippage');

-- 08.04.2015
-- Added one more attribute.

INSERT INTO attribute(name) VALUES ('partial');
INSERT INTO feature (name, description, nature_id, type) VALUES ('Sequence tagged site', 'Short, single-copy DNA sequence that characterizes a mapping landmark on the genome and can be detected by PCR; a region of the genome can be mapped by determining the order of a series of STSs.', 1, 'STS');

-- 09.04.2015
-- Deleted unique constraint on subsequence table.

ALTER TABLE subsequence DROP CONSTRAINT uk_subsequence;

-- 13.04.2015
-- And added new feature.

INSERT INTO feature (name, description, nature_id, type) VALUES ('Origin of replication', 'Starting site for duplication of nucleic acid to give two identical copies.', 1, 'rep_origin');

-- 19.04.2015
-- Added unique keys for characteristics.

ALTER TABLE accordance_characteristic ADD CONSTRAINT uk_accordance_characteristic UNIQUE (first_chain_id, second_chain_id, first_element_id, second_element_id, characteristic_type_link_id);
ALTER TABLE binary_characteristic ADD CONSTRAINT uk_binary_characteristic UNIQUE (chain_id, first_element_id, second_element_id, characteristic_type_link_id);
ALTER TABLE characteristic ADD CONSTRAINT uk_characteristic UNIQUE (chain_id, characteristic_type_link_id);
ALTER TABLE congeneric_characteristic ADD CONSTRAINT uk_congeneric_characteristic UNIQUE (chain_id, element_id, characteristic_type_link_id);

-- 21.04.2015
-- Added new attributes.

INSERT INTO attribute(name) VALUES ('codon_recognized');
INSERT INTO attribute(name) VALUES ('bound_moiety');
INSERT INTO attribute(name) VALUES ('rpt_unit_range');
INSERT INTO attribute(name) VALUES ('rpt_unit_seq');
INSERT INTO attribute(name) VALUES ('function');

-- 22.04.2015
-- Added new feature.

INSERT INTO feature (name, description, nature_id, type) VALUES ('Signal peptide coding sequence', 'Coding sequence for an N-terminal domain of a secreted protein; this domain is involved in attaching nascent polypeptide to the membrane leader sequence.', 1, 'sig_peptide');

-- 23.04.2015
-- Added another feature.

INSERT INTO feature (name, description, nature_id, type) VALUES ('Miscellaneous binding', 'Site in nucleic acid which covalently or non-covalently binds another moiety that cannot be described by any other binding key (primer_bind or protein_bind).', 1, 'misc_binding');

-- 30.04.2015
-- Added new attribute.

INSERT INTO attribute(name) VALUES ('transl_except');
INSERT INTO attribute(name) VALUES ('pseudogene');
INSERT INTO attribute(name) VALUES ('mobile_element_type');

-- 21.05.2015
-- Changing music tables structure.

DROP TRIGGER tgiu_pitch_modified ON pitch;
ALTER TABLE pitch DROP CONSTRAINT fk_pitch_note;
ALTER TABLE pitch DROP COLUMN note_id;
ALTER TABLE pitch DROP COLUMN created;
ALTER TABLE pitch DROP COLUMN modified;


CREATE TABLE note_pitch
(
   note_id bigint NOT NULL, 
   pitch_id integer NOT NULL, 
   CONSTRAINT fk_note_pitch_note FOREIGN KEY (note_id) REFERENCES note (id) ON UPDATE CASCADE ON DELETE CASCADE, 
   CONSTRAINT fk_note_pitch_pitch FOREIGN KEY (pitch_id) REFERENCES pitch (id) ON UPDATE RESTRICT ON DELETE RESTRICT
);

COMMENT ON TABLE note_pitch IS 'M:M note with pitch.';

ALTER TABLE note_pitch ADD CONSTRAINT pk_note_pitch PRIMARY KEY (note_id, pitch_id);
ALTER TABLE pitch ADD CONSTRAINT uk_pitch UNIQUE (octave, instrument_id, accidental_id, note_symbol_id);

-- 29.05.2015
-- Changing music tables structure.

ALTER TABLE pitch ALTER COLUMN instrument_id DROP NOT NULL;
COMMENT ON COLUMN pitch.instrument_id IS 'Номер музыкального инструмента.';

ALTER TABLE note  DROP COLUMN ticks;
ALTER TABLE note ADD CONSTRAINT uk_note UNIQUE (value);

-- 30.05.2015
-- Added uk for characteristic_type class name.
 
ALTER TABLE characteristic_type ADD CONSTRAINT uk_characteristic_type_class_name UNIQUE (class_name);

-- 06.06.2015
-- Added tie records.

INSERT INTO tie(id, name, description) VALUES(0, 'None', 'No tie on note');
INSERT INTO tie(id, name, description) VALUES(1, 'Start', 'On note tie starts');
INSERT INTO tie(id, name, description) VALUES(2, 'Stop', 'On note tie ends');
INSERT INTO tie(id, name, description) VALUES(3, 'StartStop', 'Note inside tie');

-- 20.07.2015
-- Added compliance charactrictic.

SELECT create_chatacteristic_type('Mutual compliance degree', 'Geometric mean of two partial compliances degrees', NULL, 'MutualComplianceDegree', false, false, false, true, true);
UPDATE characteristic_type SET name = 'Partial compliance degree' class_name = 'PartialComplianceDegree' WHERE id = 48;

-- 30.07.2015
-- Translating all db records.

UPDATE characteristic_type SET name = 'Alphabet cardinality', description = 'Count of elements in alphabet of sequence' WHERE id = 1;
UPDATE characteristic_type SET name = 'Intervals arithmetic mean', description = 'Average arithmetical value of intervals lengthes' WHERE id = 2;
UPDATE characteristic_type SET name = 'Average remoteness', description = 'Remoteness mean of congeneric sequences' WHERE id = 3;
UPDATE characteristic_type SET description = 'Count of elements in sequence (equals to length if sequence is full)' WHERE id = 4;
UPDATE characteristic_type SET description = 'Sadovsky cutting length of l-gramms for unambiguous recovery of source sequence' WHERE id = 5;
UPDATE characteristic_type SET description = 'Vocabulary entropy for sadovsky cutting length' WHERE id = 6;
UPDATE characteristic_type SET name = 'Descriptive information', description = 'Mazurs descriptive informations count' WHERE id = 7;
UPDATE characteristic_type SET name = 'Depth', description = 'Base 2 logarithm of volume characteristic' WHERE id = 8;
UPDATE characteristic_type SET description = 'Average geometric value of intervals lengthes' WHERE id = 9;
UPDATE characteristic_type SET name = 'Entropy', description = 'Shannon information or amount of information or count of identification informations' WHERE id = 10;
UPDATE characteristic_type SET name = 'Intervals count', description = 'Count of intervals in sequence' WHERE id = 11;
UPDATE characteristic_type SET name = 'Sequence length', description = 'Length of sequence measured in elements' WHERE id = 12;
DELETE FROM characteristic_type WHERE id = 13;
UPDATE characteristic_type SET name = 'Periodicity', description = 'Calculated as geometric mean divided by arithmetic mean' WHERE id = 14;
UPDATE characteristic_type SET name = 'Variations count', description = 'Number of probable sequences that can be generated from given ambiguous sequence', class_name = 'VariationsCount' WHERE id = 15;
UPDATE characteristic_type SET name = 'Frequency', description = 'Or probability' WHERE id = 16;
UPDATE characteristic_type SET name = 'Regularity', description = 'Calculated as geometric mean divided by descriptive informations count' WHERE id = 17;
UPDATE characteristic_type SET name = 'Volume', description = 'Calculated as product of all intervals in sequence' WHERE id = 18;
UPDATE characteristic_type SET name = 'Redundancy', description = 'Redundancy of coding second element with intervals between itself compared to coding with intervals from first element occurrences' WHERE id = 20;
UPDATE characteristic_type SET name = 'Partial dependence coefficient', description = 'Asymmetric measure of dependence in binary-congeneric sequence' WHERE id = 21;
UPDATE characteristic_type SET name = 'Involved partial dependence coefficient', description = 'Partial dependence coefficient weighted with frequency of elements and their pairs' WHERE id = 22;
UPDATE characteristic_type SET name = 'Mutual dependence coefficient', description = 'Geometric mean of involved partial dependence coefficients' WHERE id = 23;
UPDATE characteristic_type SET name = 'Normalized partial dependence coefficient', description = 'Partial dependence coefficient weighted with sequence length' WHERE id = 24;
UPDATE characteristic_type SET name = 'Intervals sum', description = 'Sum of intervals lengthes' WHERE id = 25;
UPDATE characteristic_type SET name = 'Alphabetic average remoteness', description = 'Average remoteness calculated with logarithm base equals to alphabet cardinality' WHERE id = 26;
UPDATE characteristic_type SET name = 'Alphabetic depth', description = 'Depth calculated with logarithm base equals to alphabet cardinality' WHERE id = 27;
UPDATE characteristic_type SET name = 'Average remoteness dispersion', description = 'Dispersion of remotenesses of congeneric sequences around average remoteness' WHERE id = 28;
UPDATE characteristic_type SET name = 'Average remoteness standard deviation', description = 'Scatter of remotenesses of congeneric sequences around average remoteness' WHERE id = 29;
UPDATE characteristic_type SET name = 'Average remoteness skewness', description = 'Asymmetry of remotenesses of congeneric sequences compared to average remoteness' WHERE id = 30;

ALTER SEQUENCE link_id_seq RESTART WITH 6;

UPDATE feature SET description = 'Complete genetic sequence' WHERE id = 1;
UPDATE feature SET description = 'Complete literary work' WHERE id = 2;
UPDATE feature SET name = 'Complete musical composition', description = 'Complete piece of music' WHERE id = 3;

UPDATE language SET name = 'Russian', description = 'Set if literary work completely or mostly written in russian language' WHERE id = 1;
UPDATE language SET name = 'English', description = 'Set if literary work completely or mostly written in english language' WHERE id = 2;
UPDATE language SET name = 'German', description = 'Set if literary work completely or mostly written in german language' WHERE id = 3;

UPDATE link SET name = 'None', description = 'First and last intervals to boundaries of sequence are not taken into account' WHERE id = 1;
UPDATE link SET name = 'To beginning', description = 'Interval from start of sequence to first occurrence of element is taken into account' WHERE id = 2;
UPDATE link SET name = 'To end', description = 'Interval from last occurrence of element to end of sequence is taken into account' WHERE id = 3;
UPDATE link SET name = 'To beginning and to end', description = 'Both intervals from start of sequence to first occurrence of element and from last occurrence of element to end of sequence are taken into account' WHERE id = 4;
UPDATE link SET name = 'Cyclic', description = 'Interval from last occurrence of element to from start of sequence to first occurrence of element (as if sequence was cyclic) is taken into account' WHERE id = 5;
INSERT INTO link(name, description) VALUES('Cyclic to beginning', 'Cyclic reading from left to right (intergals are bound to the right position (element occurrence))');
INSERT INTO link(name, description) VALUES('Cyclic to end', 'Cyclic reading from right to left (intergals are bound to the left position (element occurrence))');

UPDATE nature SET name = 'Genetic', description = 'Genetic texts, nucleotides, codons, aminoacids, segmented genetic words, etc.' WHERE id = 1;
UPDATE nature SET name = 'Music', description = 'Musical compositions, note, measures, formal motives, etc.' WHERE id = 2;
UPDATE nature SET name = 'Lirerature', description = 'Literary works, letters, words, etc.' WHERE id = 3;

UPDATE notation SET name = 'Nucleotides', description = 'Basic blocks of nucleic acids' WHERE id = 1;
UPDATE notation SET name = 'Triplets', description = 'Codons, groups of 3 nucleotides' WHERE id = 2;
UPDATE notation SET name = 'Amino acids', description = 'Basic components of peptides' WHERE id = 3;
UPDATE notation SET name = 'Genetic words', description = 'Joined sequence of nucleotides - result of segmentation of genetic sequence' WHERE id = 4;
UPDATE notation SET name = 'Normalized words', description = 'Words in normalized notation' WHERE id = 5;
UPDATE notation SET name = 'Formal motives', description = 'Joined sequence of notes - result of segmentation of musical composition' WHERE id = 6;
UPDATE notation SET name = 'Measures', description = 'Sequences of notes' WHERE id = 7;
UPDATE notation SET name = 'Notes', description = 'Basic elements of musical composition' WHERE id = 8;
UPDATE notation SET name = 'Letters', description = 'Basic elements of literary work' WHERE id = 9;

UPDATE remote_db SET name = 'GenBank / NCBI', description = 'National center for biotechnological information' WHERE id = 1;

UPDATE element SET name = 'Adenin' WHERE id = 1;
UPDATE element SET name = 'Guanine' WHERE id = 2;
UPDATE element SET name = 'Cytosine' WHERE id = 3;
UPDATE element SET name = 'Thymine' WHERE id = 4;
UPDATE element SET name = 'Uracil ' WHERE id = 5;
UPDATE element SET name = 'Glycine' WHERE id = 6;
UPDATE element SET name = 'Alanine' WHERE id = 7;
UPDATE element SET name = 'Valine' WHERE id = 8;
UPDATE element SET name = 'Isoleucine' WHERE id = 9;
UPDATE element SET name = 'Leucine' WHERE id = 10;
UPDATE element SET name = 'Proline' WHERE id = 11;
UPDATE element SET name = 'Serine' WHERE id = 12;
UPDATE element SET name = 'Threonine' WHERE id = 13;
UPDATE element SET name = 'Cysteine' WHERE id = 14;
UPDATE element SET name = 'Methionine' WHERE id = 15;
UPDATE element SET name = 'Aspartic acid' WHERE id = 16;
UPDATE element SET name = 'Asparagine' WHERE id = 17;
UPDATE element SET name = 'Glutamic acid' WHERE id = 18;
UPDATE element SET name = 'Glutamine' WHERE id = 19;
UPDATE element SET name = 'Lysine' WHERE id = 20;
UPDATE element SET name = 'Arginine' WHERE id = 21;
UPDATE element SET name = 'Histidine' WHERE id = 22;
UPDATE element SET name = 'Phenylalanine' WHERE id = 23;
UPDATE element SET name = 'Tyrosine' WHERE id = 24;
UPDATE element SET name = 'Tryptophan' WHERE id = 25;
UPDATE element SET name = 'Stop codon' WHERE id = 26;

-- 08.09.2015
-- Fixing typo.

UPDATE feature SET name = 'Complete genome' WHERE id = 1;

-- 24.09.2015
-- Add new attribute.

INSERT INTO attribute(name) VALUES ('experiment');

-- 30.09.2015
-- Add new attribute and features.

INSERT INTO attribute(name) VALUES ('citation');
INSERT INTO feature (name, description, nature_id, type) VALUES ('Stem loop', 'Hairpin; a double-helical region formed by base-pairing between adjacent (inverted) complementary sequences in a single strand of RNA or DNA.', 1, 'stem_loop');
INSERT INTO feature (name, description, nature_id, type) VALUES ('Displacement loop', 'A region within mitochondrial DNA in which a short stretch of RNA is paired with one strand of DNA, displacing the original partner DNA strand in this region; also used to describe the displacement of a region of one strand of duplex DNA by a single stranded invader in the reaction catalyzed by RecA protein.', 1, 'D-loop');
INSERT INTO feature (name, description, nature_id, type) VALUES ('Diversity segment', 'Diversity segment of immunoglobulin heavy chain, and T-cell receptor beta chain.', 1, 'D_segment');

-- 07.01.2016
-- Removing characteristics' string value.

ALTER TABLE accordance_characteristic  DROP COLUMN value_string; 
ALTER TABLE binary_characteristic  DROP COLUMN value_string; 
ALTER TABLE characteristic  DROP COLUMN value_string; 
ALTER TABLE congeneric_characteristic  DROP COLUMN value_string; 

-- 08.01.2016
-- Adding index on characteristics.

CREATE INDEX ix_characteristic_chain_characteristic_type ON characteristic (chain_id, characteristic_type_link_id);

-- 10.01.2016
-- Adding another index on characteristics table.

CREATE INDEX ix_characteristic_characteristic_type_link ON characteristic (characteristic_type_link_id);

-- 15.01.2016
-- Adding new attributes and features.

INSERT INTO attribute(name) VALUES ('regulatory_class');
INSERT INTO attribute(name) VALUES ('artificial_location');
INSERT INTO attribute(name) VALUES ('proviral');
INSERT INTO attribute(name) VALUES ('operon');
INSERT INTO attribute(name) VALUES ('number');

INSERT INTO feature (name, description, nature_id, type) VALUES ('Mobile element', 'Region of genome containing mobile elements.', 1, 'mobile_element');
INSERT INTO feature (name, description, nature_id, type) VALUES ('Variation', 'A related strain contains stable mutations from the same gene (e.g., RFLPs, polymorphisms, etc.) which differ from the presented sequence at this location (and possibly others). Used to describe alleles, RFLPs,and other naturally occurring mutations and  polymorphisms; variability arising as a result of genetic manipulation (e.g. site directed mutagenesis) should described with the misc_difference feature.', 1, 'variation');
INSERT INTO feature (name, description, nature_id, type) VALUES ('Protein_bind', 'Non-covalent protein binding site on nucleic acid. Note that feature key regulatory with /regulatory_class="ribosome_binding_site" should be used for ribosome binding sites.', 1, 'protein_bind');

-- 17.01.2016
-- Adding new feature.

INSERT INTO feature (name, description, nature_id, type) VALUES ('Mature peptid', 'Mature peptide or protein coding sequence; coding sequence for the mature or final peptide or protein product following post-translational modification; the location does not include the stop codon (unlike the corresponding CDS).', 1, 'mat_peptide');
INSERT INTO feature (name, description, nature_id, type) VALUES ('Miscellaneous difference', 'feature sequence is different from that presented in the entry and cannot be described by any other difference key (old_sequence, variation, or modified_base).', 1, 'misc_difference');
INSERT INTO attribute(name) VALUES ('replace');
INSERT INTO attribute(name) VALUES ('compare');

-- 18.01.2016 
-- Updating characteristics trigger function

CREATE OR REPLACE FUNCTION trigger_delete_chain_characteristics()
  RETURNS trigger AS
$BODY$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "INSERT" || TG_OP == "UPDATE"){
	plv8.execute('DELETE FROM characteristic USING chain c WHERE characteristic.chain_id = c.id AND characteristic.created < c.modified;');
	plv8.execute('DELETE FROM binary_characteristic USING chain c WHERE binary_characteristic.chain_id = c.id AND binary_characteristic.created < c.modified;');
	plv8.execute('DELETE FROM congeneric_characteristic USING chain c WHERE congeneric_characteristic.chain_id = c.id AND congeneric_characteristic.created < c.modified;');
	plv8.execute('DELETE FROM accordance_characteristic USING chain c WHERE accordance_characteristic.chain_id = c.id AND accordance_characteristic.created < c.modified;');
} else{
	plv8.elog(ERROR, 'Неизвестная операция. Данный тригер предназначен только для операций добавления и изменения записей.');
}

$BODY$
  LANGUAGE plv8 VOLATILE
  COST 100;
COMMENT ON FUNCTION trigger_delete_chain_characteristics() IS 'Триггерная функция, удаляющая все характеристики при удалении или изменении цепочки.'; 

-- 25.01.2016
-- Removing characteristic type creation function and new characteristic type.

DROP FUNCTION create_chatacteristic_type(character varying, text, integer, character varying, boolean, boolean, boolean, boolean, boolean);

INSERT INTO characteristic_type (name, class_name, full_chain_applicable, congeneric_chain_applicable, binary_chain_applicable, accordance_applicable) VALUES ('Uniformity', 'Uniformity', true, true, false, false);
INSERT INTO characteristic_type_link (characteristic_type_id, link_id) (SELECT max(id), 1 FROM characteristic_type);
INSERT INTO characteristic_type_link (characteristic_type_id, link_id) (SELECT max(id), 2 FROM characteristic_type);
INSERT INTO characteristic_type_link (characteristic_type_id, link_id) (SELECT max(id), 3 FROM characteristic_type);
INSERT INTO characteristic_type_link (characteristic_type_id, link_id) (SELECT max(id), 4 FROM characteristic_type);
INSERT INTO characteristic_type_link (characteristic_type_id, link_id) (SELECT max(id), 5 FROM characteristic_type);

-- 01.02.2016
-- Removing redundant "complement" columns.

ALTER TABLE dna_chain DROP COLUMN complementary;
ALTER TABLE subsequence DROP COLUMN complementary;

-- 06.03.2016
-- Removing web api id coumns. Adding remote_id column into subsequences table. And fixed delete characteristics function. 

CREATE OR REPLACE FUNCTION trigger_delete_chain_characteristics()
  RETURNS trigger AS
$BODY$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "UPDATE"){
	plv8.execute('DELETE FROM characteristic USING chain c WHERE characteristic.chain_id = c.id AND characteristic.created < c.modified;');
	plv8.execute('DELETE FROM binary_characteristic USING chain c WHERE binary_characteristic.chain_id = c.id AND binary_characteristic.created < c.modified;');
	plv8.execute('DELETE FROM congeneric_characteristic USING chain c WHERE congeneric_characteristic.chain_id = c.id AND congeneric_characteristic.created < c.modified;');
	plv8.execute('DELETE FROM accordance_characteristic USING chain c WHERE (accordance_characteristic.first_chain_id = c.id OR accordance_characteristic.second_chain_id = c.id) AND accordance_characteristic.created < c.modified;');
} else{
	plv8.elog(ERROR, 'Unknown operation: ' + TG_OP + '. This trigger only works on UPDATE operation.');
}

$BODY$
  LANGUAGE plv8 VOLATILE
  COST 100;
COMMENT ON FUNCTION trigger_delete_chain_characteristics() IS 'Trigger function deleting all characteristics of sequences that has been updated.';

ALTER TABLE dna_chain DROP COLUMN web_api_id;
ALTER TABLE subsequence DROP COLUMN web_api_id;
ALTER TABLE subsequence  ADD COLUMN remote_id character varying(255);

UPDATE subsequence s SET remote_id = c.value FROM chain_attribute c WHERE c.attribute_id = 2 AND c.chain_id = s.id;

ALTER TABLE chain_attribute DROP CONSTRAINT uk_chain_attribute;
ALTER TABLE chain_attribute ADD CONSTRAINT uk_chain_attribute UNIQUE(chain_id, attribute_id, value);

-- 14.03.2016
-- Added new feature.

INSERT INTO feature (name, description, nature_id, type) VALUES ('Gene (non coding)', 'Gene without CDS (coding sequence) associated with it.', 1, 'gene');

-- 26.06.2016
-- Added new features.

INSERT INTO feature (name, description, nature_id, type) VALUES ('3 end', 'Region at the 3 end of a mature transcript (following the stop codon) that is not translated into a protein; region at the 3 end of an RNA virus (following the last stop codon) that is not translated into a protein.', 1, '3''UTR');
INSERT INTO feature (name, description, nature_id, type) VALUES ('5 end', 'Region at the 5 end of a mature transcript (preceding the initiation codon) that is not translated into a protein;region at the 5 end of an RNA virus genome (preceding the first initiation codon) that is not translated into a protein.', 1, '5''UTR');
INSERT INTO feature (name, description, nature_id, type) VALUES ('Primer bind', 'Non-covalent primer binding site for initiation of replication, transcription, or reverse transcription; includes site(s) for synthetic e.g., PCR primer elements.', 1, 'primer_bind');
	
	
-- 02.07.2016
-- Removing not used column in chain.

ALTER TABLE chain DROP COLUMN piece_position;
ALTER TABLE data_chain DROP COLUMN piece_position;

-- 06.11.2016
-- Added new feature

INSERT INTO feature (name, description, nature_id, complete) VALUES ('Plastid', 'Plastid genome', 1, true);

-- 09.12.2016
-- Added sequence type column to matter table.

ALTER TABLE matter ADD COLUMN sequence_type smallint;
COMMENT ON COLUMN matter.sequence_type IS 'Reference to SequrnceType enum.';

UPDATE matter SET sequence_type = CASE 
	WHEN nature_id IN (2, 3, 4) THEN nature_id
	WHEN nature_id = 1 AND (description LIKE '%Plasmid%'  OR description LIKE '%plasmid%') THEN 5
	WHEN nature_id = 1 AND (description LIKE '%Mitochondrion%'  OR description LIKE '%mitochondrion%' OR description LIKE '%mitochondrial%' OR description LIKE '%Mitochondrial%') THEN 6
	WHEN nature_id = 1 AND (description LIKE '%Chloroplast%'  OR description LIKE '%chloroplast%') THEN 7
    WHEN nature_id = 1 AND description LIKE '%16S%' THEN 8
    WHEN nature_id = 1 AND description LIKE '%18S%' THEN 9
	ELSE 1
END;

ALTER TABLE matter ALTER COLUMN sequence_type SET NOT NULL;


ALTER TABLE matter ADD COLUMN "group" smallint;
COMMENT ON COLUMN matter.group IS 'Reference to Group enum.';

UPDATE matter SET "group" = CASE 
	WHEN nature_id IN (2, 3, 4) THEN nature_id
	WHEN nature_id = 1 AND (name LIKE '%virus%'  OR name LIKE '%Virus%') THEN 5
    WHEN nature_id = 1 AND description LIKE '%18S%' THEN 6
	ELSE 1
END;

ALTER TABLE matter ALTER COLUMN "group" SET NOT NULL;

-- 23.12.2016
-- Removing feature id and feature references from all tables except subsequnce.

ALTER TABLE data_chain DROP CONSTRAINT fk_data_chain_piece_type;
ALTER TABLE chain DROP CONSTRAINT fk_chain_piece_type;
ALTER TABLE dna_chain DROP CONSTRAINT fk_dna_chain_piece_type;
ALTER TABLE fmotiv DROP CONSTRAINT fk_fmotiv_piece_type;
ALTER TABLE literature_chain DROP CONSTRAINT fk_literature_chain_piece_type;
ALTER TABLE measure DROP CONSTRAINT fk_measure_piece_type;
ALTER TABLE music_chain DROP CONSTRAINT fk_music_chain_piece_type;
ALTER TABLE chain DROP COLUMN feature_id;
ALTER TABLE dna_chain DROP COLUMN fasta_header;

-- 10.01.2017
-- Removing redundant features used for cemplete sequences.
-- And removing redundant column "complete". 
DELETE FROM feature WHERE id IN (1,2,3,10,11,12,15,17,37);
ALTER TABLE feature DROP COLUMN complete;

-- 11.01.2017
-- Updating feature ids.
ALTER TABLE subsequence DROP CONSTRAINT fk_subsequence_feature;
ALTER TABLE subsequence ADD CONSTRAINT fk_subsequence_feature FOREIGN KEY (feature_id) REFERENCES feature (id) ON UPDATE CASCADE ON DELETE NO ACTION;

UPDATE feature SET id = 0 WHERE id = 14;
UPDATE feature SET id = 1 WHERE id = 4;
UPDATE feature SET id = 2 WHERE id = 5;
UPDATE feature SET id = 3 WHERE id = 6;
UPDATE feature SET id = 4 WHERE id = 7;
UPDATE feature SET id = 5 WHERE id = 8;
UPDATE feature SET id = 6 WHERE id = 9;
UPDATE feature SET id = 7 WHERE id = 13;
UPDATE feature SET id = 8 WHERE id = 16;
UPDATE feature SET id = 9 WHERE id = 18;
UPDATE feature SET id = 10 WHERE id = 19;
UPDATE feature SET id = 11 WHERE id = 20;
UPDATE feature SET id = 12 WHERE id = 21;
UPDATE feature SET id = 13 WHERE id = 22;
UPDATE feature SET id = 14 WHERE id = 23;
UPDATE feature SET id = 15 WHERE id = 24;
UPDATE feature SET id = 16 WHERE id = 25;
UPDATE feature SET id = 17 WHERE id = 26;
UPDATE feature SET id = 18 WHERE id = 27;
UPDATE feature SET id = 19 WHERE id = 28;
UPDATE feature SET id = 20 WHERE id = 29;
UPDATE feature SET id = 21 WHERE id = 30;
UPDATE feature SET id = 22 WHERE id = 31;
UPDATE feature SET id = 23 WHERE id = 32;
UPDATE feature SET id = 24 WHERE id = 33;
UPDATE feature SET id = 25 WHERE id = 34;
UPDATE feature SET id = 26 WHERE id = 35;
UPDATE feature SET id = 27 WHERE id = 36;

-- 11.01.2017
-- Added new default instrument and translator.

INSERT INTO instrument (id,name,description) VALUES(0, 'Any or unknown', 'Any or unknown instrument');
INSERT INTO translator (id,name,description) VALUES(0, 'None or manual', 'No translator is applied (text is original) or text translated manualy');

ALTER TABLE literature_chain DROP CONSTRAINT chk_original_translator;
ALTER TABLE literature_chain ADD CONSTRAINT chk_original_translator CHECK (original AND translator_id = 0 OR NOT original);
UPDATE literature_chain SET translator_id = 0 WHERE translator_id IS NULL;

ALTER TABLE literature_chain ALTER COLUMN translator_id SET DEFAULT 0;
ALTER TABLE literature_chain ALTER COLUMN translator_id SET NOT NULL;
ALTER TABLE pitch ALTER COLUMN instrument_id SET DEFAULT 0;
ALTER TABLE pitch ALTER COLUMN instrument_id SET NOT NULL;

-- 11.01.2017
-- Added fmotiv types.

INSERT INTO fmotiv_type (id,name,description) VALUES(1, 'Complete minimal measure', '');
INSERT INTO fmotiv_type (id,name,description) VALUES(2, 'Partial minimal measure', '');
INSERT INTO fmotiv_type (id,name,description) VALUES(3, 'Increasing sequence', '');
INSERT INTO fmotiv_type (id,name,description) VALUES(4, 'Complete minimal metrorhythmic group', 'One of two subtypes of minimal metrorhythmic group with complete minimal measure at the begining');
INSERT INTO fmotiv_type (id,name,description) VALUES(5, 'Partial minimal metrorhythmic group', 'One of two subtypes of minimal metrorhythmic group with partial minimal measure at the begining');

-- 13.01.2017
-- Changing types of static table's ids to smallint.

ALTER TABLE data_chain DROP COLUMN feature_id;

ALTER TABLE translator ALTER COLUMN id TYPE smallint;
ALTER TABLE tie ALTER COLUMN id TYPE smallint;
ALTER TABLE remote_db ALTER COLUMN id TYPE smallint;
ALTER TABLE note_symbol ALTER COLUMN id TYPE smallint;
ALTER TABLE notation ALTER COLUMN id TYPE smallint;
ALTER TABLE nature ALTER COLUMN id TYPE smallint;
ALTER TABLE link ALTER COLUMN id TYPE smallint;
ALTER TABLE "language" ALTER COLUMN id TYPE smallint;
ALTER TABLE instrument ALTER COLUMN id TYPE smallint;
ALTER TABLE fmotiv_type ALTER COLUMN id TYPE smallint;
ALTER TABLE attribute ALTER COLUMN id TYPE smallint;
ALTER TABLE accidental ALTER COLUMN id TYPE smallint;

ALTER TABLE subsequence ALTER COLUMN feature_id TYPE smallint;
ALTER TABLE pitch ALTER COLUMN instrument_id TYPE smallint;
ALTER TABLE pitch ALTER COLUMN accidental_id TYPE smallint;
ALTER TABLE pitch ALTER COLUMN note_symbol_id TYPE smallint;
ALTER TABLE note ALTER COLUMN tie_id TYPE smallint;
ALTER TABLE notation ALTER COLUMN nature_id TYPE smallint;
ALTER TABLE matter ALTER COLUMN nature_id TYPE smallint;
ALTER TABLE literature_chain ALTER COLUMN language_id TYPE smallint;
ALTER TABLE literature_chain ALTER COLUMN translator_id TYPE smallint;
ALTER TABLE fmotiv ALTER COLUMN fmotiv_type_id TYPE smallint;
ALTER TABLE feature ALTER COLUMN nature_id TYPE smallint;
ALTER TABLE element ALTER COLUMN notation_id TYPE smallint;
ALTER TABLE characteristic_type_link ALTER COLUMN link_id TYPE smallint;
ALTER TABLE chain_attribute ALTER COLUMN attribute_id TYPE smallint;
ALTER TABLE chain ALTER COLUMN notation_id TYPE smallint;
ALTER TABLE chain ALTER COLUMN remote_db_id TYPE smallint;

-- 18.01.2017
-- Change matter name type to varchar without limit.

ALTER TABLE matter ALTER COLUMN name TYPE text;

-- 18.02.2017
-- Creating new characteristic_type_link tables.

CREATE TABLE accordance_characteristic_link
(
   id smallserial NOT NULL, 
   accordance_characteristic smallint NOT NULL, 
   link smallint NOT NULL, 
   CONSTRAINT pk_accordance_characteristic_link PRIMARY KEY (id), 
   CONSTRAINT uk_accordance_characteristic_link UNIQUE (accordance_characteristic, link), 
   CONSTRAINT accordance_characteristic_check CHECK (accordance_characteristic::int4 <@ int4range(1,2, '[]')), 
   CONSTRAINT accordance_characteristic_link_check CHECK (link::int4 <@ int4range(0,7, '[]'))
);

CREATE TABLE binary_characteristic_link
(
   id smallserial NOT NULL, 
   binary_characteristic smallint NOT NULL, 
   link smallint NOT NULL, 
   CONSTRAINT pk_binary_characteristic_link PRIMARY KEY (id), 
   CONSTRAINT uk_binary_characteristic_link UNIQUE (binary_characteristic, link), 
   CONSTRAINT binary_characteristic_check CHECK (binary_characteristic::int4 <@ int4range(1,6, '[]')), 
   CONSTRAINT binary_characteristic_link_check CHECK (link::int4 <@ int4range(0,7, '[]'))
);

CREATE TABLE congeneric_characteristic_link
(
   id smallserial NOT NULL, 
   congeneric_characteristic smallint NOT NULL, 
   link smallint NOT NULL, 
   CONSTRAINT pk_congeneric_characteristic_link PRIMARY KEY (id), 
   CONSTRAINT uk_congeneric_characteristic_link UNIQUE (congeneric_characteristic, link), 
   CONSTRAINT congeneric_characteristic_check CHECK (congeneric_characteristic::int4 <@ int4range(1,18, '[]')), 
   CONSTRAINT congeneric_characteristic_link_check CHECK (link::int4 <@ int4range(0,7, '[]'))
);

CREATE TABLE full_characteristic_link
(
   id smallserial NOT NULL, 
   full_characteristic smallint NOT NULL, 
   link smallint NOT NULL, 
   CONSTRAINT pk_full_characteristic_link PRIMARY KEY (id), 
   CONSTRAINT uk_full_characteristic_link UNIQUE (full_characteristic, link), 
   CONSTRAINT full_characteristic_check CHECK (full_characteristic::int4 <@ int4range(1,54, '[]')), 
   CONSTRAINT full_characteristic_link_check CHECK (link::int4 <@ int4range(0,7, '[]'))
);

-- 24.02.2017
-- Creating accordance and binary characteristic_links

INSERT INTO accordance_characteristic_link (accordance_characteristic, link) VALUES (1,2);
INSERT INTO accordance_characteristic_link (accordance_characteristic, link) VALUES (1,3);
INSERT INTO accordance_characteristic_link (accordance_characteristic, link) VALUES (1,6);
INSERT INTO accordance_characteristic_link (accordance_characteristic, link) VALUES (1,7);

INSERT INTO accordance_characteristic_link (accordance_characteristic, link) VALUES (2,2);
INSERT INTO accordance_characteristic_link (accordance_characteristic, link) VALUES (2,3);
INSERT INTO accordance_characteristic_link (accordance_characteristic, link) VALUES (2,6);
INSERT INTO accordance_characteristic_link (accordance_characteristic, link) VALUES (2,7);


INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (1,2);
INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (1,3);
INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (1,4);

INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (2,2);
INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (2,3);
INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (2,4);

INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (3,2);
INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (3,3);
INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (3,4);

INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (4,2);
INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (4,3);
INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (4,4);

INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (5,2);
INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (5,3);
INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (5,4);

INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (6,2);
INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (6,3);
INSERT INTO binary_characteristic_link (binary_characteristic, link) VALUES (6,4);

-- 24.02.2017
-- Creating congeneric characteristic_links

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (1,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (1,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (1,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (1,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (1,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (2,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (2,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (2,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (2,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (2,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (3,0);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (4,0);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (5,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (5,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (5,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (5,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (5,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (6,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (6,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (6,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (6,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (6,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (7,0);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (8,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (8,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (8,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (8,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (8,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (9,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (9,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (9,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (9,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (9,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (10,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (10,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (10,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (10,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (10,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (11,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (11,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (11,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (11,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (11,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (12,0);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (13,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (13,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (13,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (13,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (13,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (14,0);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (15,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (15,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (15,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (15,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (15,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (16,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (16,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (16,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (16,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (16,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (17,0);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (18,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (18,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (18,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (18,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (18,5);

-- 24.02.2017
-- Creating full characteristic_links

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (1,0);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (2,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (2,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (2,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (2,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (2,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (3,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (3,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (3,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (3,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (3,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (4,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (4,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (4,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (4,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (4,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (5,0);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (6,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (6,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (6,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (6,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (6,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (7,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (7,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (7,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (7,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (7,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (8,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (8,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (8,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (8,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (8,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (9,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (9,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (9,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (9,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (9,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (10,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (10,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (10,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (10,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (10,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (11,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (11,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (11,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (11,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (11,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (12,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (12,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (12,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (12,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (12,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (13,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (13,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (13,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (13,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (13,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (14,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (14,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (14,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (14,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (14,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (15,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (15,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (15,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (15,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (15,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (16,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (16,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (16,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (16,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (16,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (17,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (17,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (17,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (17,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (17,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (18,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (18,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (18,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (18,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (18,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (19,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (19,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (19,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (19,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (19,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (20,0);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (21,0);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (22,0);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (23,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (23,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (23,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (23,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (23,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (24,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (24,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (24,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (24,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (24,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (25,0);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (26,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (26,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (26,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (26,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (26,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (27,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (27,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (27,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (27,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (27,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (28,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (28,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (28,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (28,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (28,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (29,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (29,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (29,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (29,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (29,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (30,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (30,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (30,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (30,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (30,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (31,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (31,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (31,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (31,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (31,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (32,0);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (33,0);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (34,0);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (35,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (35,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (35,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (35,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (35,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (36,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (36,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (36,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (36,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (36,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (37,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (37,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (37,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (37,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (37,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (38,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (38,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (38,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (38,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (38,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (39,0);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (40,0);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (41,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (41,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (41,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (41,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (41,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (42,0);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (43,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (43,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (43,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (43,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (43,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (44,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (44,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (44,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (44,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (44,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (45,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (45,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (45,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (45,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (45,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (46,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (46,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (46,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (46,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (46,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (47,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (47,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (47,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (47,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (47,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (48,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (48,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (48,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (48,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (48,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (49,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (49,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (49,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (49,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (49,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (50,0);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (51,0);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (52,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (52,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (52,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (52,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (52,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (53,0);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (54,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (54,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (54,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (54,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (54,5);

-- 09.03.2017
-- Adding new feature.

INSERT INTO feature (id, name, description, nature_id, type) VALUES (28, 'Intron', 'A segment of DNA that is transcribed, but removed from within the transcript by splicing together the sequences (exons) on either side of it.', 1, 'intron');

-- 16.03.2017
-- Add table for tasks.

CREATE TABLE task
(
   id bigserial NOT NULL, 
   task_type smallint NOT NULL, 
   description text NOT NULL, 
   status smallint, 
   result json, 
   user_id integer NOT NULL, 
   created timestamp with time zone NOT NULL, 
   started timestamp with time zone, 
   completed timestamp with time zone, 
   CONSTRAINT pk_task PRIMARY KEY (id), 
   CONSTRAINT fk_task_user FOREIGN KEY (user_id) REFERENCES dbo."AspNetUsers" ("Id") ON UPDATE NO ACTION ON DELETE NO ACTION
);

-- 31.03.2017
-- Delete characteristics values and update characteristics tables foreign keys.

ALTER TABLE characteristic RENAME TO full_characteristic;

DELETE FROM accordance_characteristic;
DELETE FROM binary_characteristic;
DELETE FROM congeneric_characteristic;
DELETE FROM full_characteristic;

DROP TRIGGER tgiu_accordance_characteristic_applicability ON accordance_characteristic;
DROP TRIGGER tgiu_binary_characteristic_applicability ON binary_characteristic;
DROP TRIGGER tgiu_congeneric_characteristic_applicability ON congeneric_characteristic;
DROP TRIGGER tgiu_characteristic_applicability ON full_characteristic;


ALTER TABLE accordance_characteristic DROP CONSTRAINT fk_characteristic_type_link;
ALTER TABLE accordance_characteristic ADD CONSTRAINT fk_accordance_characteristic_link FOREIGN KEY (characteristic_type_link_id) REFERENCES accordance_characteristic_link (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE NO ACTION;

ALTER TABLE binary_characteristic DROP CONSTRAINT fk_characteristic_type_link;
ALTER TABLE binary_characteristic ADD CONSTRAINT fk_binary_characteristic_link FOREIGN KEY (characteristic_type_link_id) REFERENCES binary_characteristic_link (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE NO ACTION;

ALTER TABLE congeneric_characteristic DROP CONSTRAINT fk_characteristic_type_link;
ALTER TABLE congeneric_characteristic ADD CONSTRAINT fk_congeneric_characteristic_link FOREIGN KEY (characteristic_type_link_id) REFERENCES congeneric_characteristic_link (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE NO ACTION;

ALTER TABLE full_characteristic DROP CONSTRAINT fk_characteristic_type_link;
ALTER TABLE full_characteristic ADD CONSTRAINT fk_full_characteristic_link FOREIGN KEY (characteristic_type_link_id) REFERENCES full_characteristic_link (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE NO ACTION;

-- 03.04.2017
-- Deleting "created" and "modified" fields from characteristics values tables.
-- Also droped tables characteristic_type and characteristic_type_link.

ALTER TABLE accordance_characteristic DROP COLUMN created;
ALTER TABLE accordance_characteristic DROP COLUMN modified;
ALTER TABLE binary_characteristic DROP COLUMN created;
ALTER TABLE binary_characteristic DROP COLUMN modified;
ALTER TABLE congeneric_characteristic DROP COLUMN created;
ALTER TABLE congeneric_characteristic DROP COLUMN modified;
ALTER TABLE full_characteristic DROP COLUMN created;
ALTER TABLE full_characteristic DROP COLUMN modified;

DROP TABLE characteristic_type_link;
DROP TABLE characteristic_type;

-- 04.04.2017
-- Deleting redundant triggers.
-- And update another trigger.

DROP TRIGGER tgiu_accordance_characteristic_modified ON accordance_characteristic;
DROP TRIGGER tgiu_binary_characteristic_modified ON binary_characteristic;
DROP TRIGGER tgiu_congeneric_characteristic_modified ON congeneric_characteristic;
DROP TRIGGER tgiu_characteristic_modified ON full_characteristic;
DROP FUNCTION trigger_check_applicability();

DROP TRIGGER tgu_chain_characteristics ON chain;
DROP TRIGGER tgu_data_chain_characteristics ON data_chain;
DROP TRIGGER tgu_dna_chain_characteristics ON dna_chain;
DROP TRIGGER tgu_fmotiv_characteristics ON fmotiv;
DROP TRIGGER tgu_literature_chain_characteristics ON literature_chain;
DROP TRIGGER tgu_measure_characteristics ON measure;
DROP TRIGGER tgu_music_chain_characteristics ON music_chain;
DROP TRIGGER tgu_subsequence_characteristics ON subsequence;

DROP FUNCTION trigger_delete_chain_characteristics();
CREATE OR REPLACE FUNCTION trigger_delete_chain_characteristics()
  RETURNS trigger AS
$BODY$
//plv8.elog(NOTICE, "TG_TABLE_NAME = ", TG_TABLE_NAME);
//plv8.elog(NOTICE, "TG_OP = ", TG_OP);
//plv8.elog(NOTICE, "TG_ARGV = ", TG_ARGV);

if (TG_OP == "UPDATE"){
	plv8.execute('DELETE FROM full_characteristic USING chain c WHERE full_characteristic.chain_id = c.id;');
	plv8.execute('DELETE FROM binary_characteristic USING chain c WHERE binary_characteristic.chain_id = c.id;');
	plv8.execute('DELETE FROM congeneric_characteristic USING chain c WHERE congeneric_characteristic.chain_id = c.id;');
	plv8.execute('DELETE FROM accordance_characteristic USING chain c WHERE accordance_characteristic.first_chain_id = c.id OR accordance_characteristic.second_chain_id = c.id;');
} else{
	plv8.elog(ERROR, 'Unknown operation: ' + TG_OP + '. This trigger only works on UPDATE operation.');
}

$BODY$
  LANGUAGE plv8 VOLATILE
  COST 100;
ALTER FUNCTION trigger_delete_chain_characteristics()
  OWNER TO postgres;
COMMENT ON FUNCTION trigger_delete_chain_characteristics() IS 'Trigger function deleting all characteristics of sequences that has been updated.';

CREATE TRIGGER tgu_subsequence_characteristics AFTER UPDATE ON subsequence FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_subsequence_characteristics ON subsequence IS 'Trigger deleting all characteristics of sequences that has been updated.';
CREATE TRIGGER tgu_music_chain_characteristics AFTER UPDATE ON music_chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_music_chain_characteristics ON music_chain IS 'Trigger deleting all characteristics of sequences that has been updated.';
CREATE TRIGGER tgu_measure_characteristics AFTER UPDATE ON measure FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_measure_characteristics ON measure IS 'Trigger deleting all characteristics of sequences that has been updated.';
CREATE TRIGGER tgu_literature_chain_characteristics AFTER UPDATE ON literature_chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_literature_chain_characteristics ON literature_chain IS 'Trigger deleting all characteristics of sequences that has been updated.';
CREATE TRIGGER tgu_fmotiv_characteristics AFTER UPDATE ON fmotiv FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_fmotiv_characteristics ON fmotiv IS 'Trigger deleting all characteristics of sequences that has been updated.';
CREATE TRIGGER tgu_dna_chain_characteristics AFTER UPDATE ON dna_chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_dna_chain_characteristics ON dna_chain IS 'Trigger deleting all characteristics of sequences that has been updated.';
CREATE TRIGGER tgu_data_chain_characteristics AFTER UPDATE ON data_chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_data_chain_characteristics ON data_chain IS 'Trigger deleting all characteristics of sequences that has been updated.';
CREATE TRIGGER tgu_chain_characteristics AFTER UPDATE ON chain FOR EACH STATEMENT EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_chain_characteristics ON chain IS 'Trigger deleting all characteristics of sequences that has been updated.';

-- 09.04.2017
-- Deleting static tables as they all replaced with enums.
-- And updating references to this tables.

ALTER TABLE measure NO INHERIT chain;
ALTER TABLE fmotiv NO INHERIT chain;

ALTER TABLE pitch RENAME instrument_id TO instrument;
ALTER TABLE pitch RENAME accidental_id TO accidental;
ALTER TABLE pitch ALTER COLUMN accidental SET DEFAULT 0;
ALTER TABLE pitch RENAME note_symbol_id TO note_symbol;
ALTER TABLE pitch DROP CONSTRAINT fk_pitch_accidental;
ALTER TABLE pitch DROP CONSTRAINT fk_pitch_instrument;
ALTER TABLE pitch DROP CONSTRAINT fk_pitch_note_symbol;
COMMENT ON COLUMN pitch.note_symbol IS 'Note symbol.';
COMMENT ON COLUMN pitch.instrument IS 'Instrument of pitch.';
COMMENT ON COLUMN pitch.accidental IS 'Accidental of pitch.';

ALTER TABLE note RENAME tie_id TO tie;
ALTER TABLE note ALTER COLUMN tie SET DEFAULT 0;
ALTER TABLE note DROP CONSTRAINT fk_note_tie;
ALTER TABLE note DROP CONSTRAINT fk_note_notation;
COMMENT ON COLUMN note.tie IS 'Tie type of the note.';

ALTER TABLE literature_chain RENAME language_id TO language;
ALTER TABLE literature_chain RENAME translator_id TO translator;
ALTER TABLE literature_chain DROP CONSTRAINT fk_litarure_chain_translator;
ALTER TABLE literature_chain DROP CONSTRAINT fk_literature_chain_language;
ALTER TABLE literature_chain DROP CONSTRAINT fk_literature_chain_notation;
ALTER TABLE literature_chain DROP CONSTRAINT fk_literature_chain_remote_db;
COMMENT ON COLUMN literature_chain.language IS 'Primary language of literary work.';
COMMENT ON COLUMN literature_chain.translator IS 'Author of translation or automated translator.';

ALTER TABLE subsequence RENAME feature_id TO feature;
ALTER TABLE subsequence DROP CONSTRAINT fk_subsequence_feature;

ALTER TABLE music_chain DROP CONSTRAINT fk_music_chain_notation;
ALTER TABLE music_chain DROP CONSTRAINT fk_music_chain_remote_db;

ALTER TABLE measure DROP CONSTRAINT fk_measure_notation;
ALTER TABLE measure DROP CONSTRAINT fk_measure_remote_db;

ALTER TABLE matter RENAME nature_id TO nature;
ALTER TABLE matter DROP CONSTRAINT fk_matter_nature;
COMMENT ON COLUMN matter.nature IS 'Nature of the object.';

ALTER TABLE fmotiv RENAME fmotiv_type_id TO fmotiv_type;
ALTER TABLE fmotiv DROP CONSTRAINT fk_fmotiv_fmotiv_type;
ALTER TABLE fmotiv DROP CONSTRAINT fk_fmotiv_notation;
ALTER TABLE fmotiv DROP CONSTRAINT fk_fmotiv_remote_db;
COMMENT ON COLUMN fmotiv.fmotiv_type IS 'Type of f motiv.';

ALTER TABLE element RENAME notation_id  TO notation;
ALTER TABLE element DROP CONSTRAINT fk_element_notation;
COMMENT ON COLUMN element.notation IS 'Notation of the element.';

ALTER TABLE dna_chain DROP CONSTRAINT fk_dna_chain_notation;
ALTER TABLE dna_chain DROP CONSTRAINT fk_dna_chain_remote_db;

ALTER TABLE data_chain DROP CONSTRAINT fk_data_chain_notation;
ALTER TABLE data_chain DROP CONSTRAINT fk_data_chain_remote_db;

ALTER TABLE chain_attribute RENAME attribute_id TO attribute;
ALTER TABLE chain_attribute DROP CONSTRAINT fk_chain_attribute_attribute;

ALTER TABLE chain RENAME notation_id TO notation;
ALTER TABLE chain RENAME remote_db_id TO remote_db;
ALTER TABLE chain DROP CONSTRAINT fk_chain_notation;
ALTER TABLE chain DROP CONSTRAINT fk_chain_remote_db;
COMMENT ON COLUMN chain.notation IS 'Notation of the sequence (words, letters, notes, nucleotides, etc.).';
COMMENT ON COLUMN chain.remote_db IS 'Remote db from whitch sequence is downloaded.';

DROP TABLE accidental;
DROP TABLE attribute;
DROP TABLE feature;
DROP TABLE fmotiv_type;
DROP TABLE instrument;
DROP TABLE language;
DROP TABLE link;
DROP TABLE notation;
DROP TABLE remote_db;
DROP TABLE nature;
DROP TABLE note_symbol;
DROP TABLE tie;
DROP TABLE translator;
DROP TABLE characteristic_group;

-- 12.04.2017
-- deleting duplicate indexes.

DROP INDEX ix_chain_key;
DROP INDEX ix_element_value_notation;
DROP INDEX ix_element_key;
DROP INDEX ix_element_id;
DROP INDEX ix_characteristic_chain_characteristic_type;
DROP INDEX ix_characteristic_id;
DROP INDEX ix_position_id;
DROP INDEX "ix-matter_name_nature";
DROP INDEX ix_literature_chain_id;
DROP INDEX ix_matter_matter_id;
DROP INDEX ix_dna_chain_id;
DROP INDEX ix_pitch;
DROP INDEX ix_music_chain_id;
DROP INDEX ix_measure_id;
DROP INDEX ix_chain_id;
DROP INDEX ix_fmotiv_id;
DROP INDEX ix_note_id;
ALTER TABLE note_pitch DROP CONSTRAINT uk_note_pitch;

-- 13.04.2017
-- Fixing characteristic_type type.

ALTER TABLE full_characteristic RENAME characteristic_type_link_id TO characteristic_link_id;
ALTER TABLE full_characteristic ALTER COLUMN characteristic_link_id TYPE smallint;
ALTER TABLE congeneric_characteristic RENAME characteristic_type_link_id TO characteristic_link_id;
ALTER TABLE congeneric_characteristic ALTER COLUMN characteristic_link_id TYPE smallint;
ALTER TABLE binary_characteristic RENAME characteristic_type_link_id TO characteristic_link_id;
ALTER TABLE binary_characteristic ALTER COLUMN characteristic_link_id TYPE smallint;
ALTER TABLE accordance_characteristic RENAME characteristic_type_link_id TO characteristic_link_id;
ALTER TABLE accordance_characteristic ALTER COLUMN characteristic_link_id TYPE smallint;

-- 20.04.2017
-- Refactor task table.

ALTER TABLE task ALTER COLUMN status SET NOT NULL;

-- 15.08.2017
-- Add indexes of new type to subsequence table.

CREATE INDEX ix_subsequence_sequence_id_brin ON subsequence USING brin (chain_id);
CREATE INDEX ix_subsequence_sequence_id_feature_brin ON subsequence USING brin (chain_id, feature);
CREATE INDEX ix_subsequence_feature_brin ON subsequence USING brin (feature);

-- 15.08.2017
-- Add indexes of new type to characteristics values tables.

CREATE INDEX ix_full_characteristic_sequence_id_brin ON full_characteristic USING brin (chain_id);
CREATE INDEX ix_full_characteristic_characteristic_link_id_brin ON full_characteristic USING brin (characteristic_link_id);

CREATE INDEX ix_congeneric_characteristic_sequence_id_brin ON congeneric_characteristic USING brin (chain_id);
CREATE INDEX ix_congeneric_characteristic_characteristic_link_id_brin ON congeneric_characteristic USING brin (characteristic_link_id);
CREATE INDEX ix_congeneric_characteristic_sequence_id_element_id_brin ON congeneric_characteristic USING brin (chain_id, element_id);
CREATE INDEX ix_congeneric_characteristic_sequence_id_characteristic_link_id_brin ON congeneric_characteristic USING brin (chain_id, characteristic_link_id);

CREATE INDEX ix_accordance_characteristic_first_sequence_id_brin ON accordance_characteristic USING brin (first_chain_id);
CREATE INDEX ix_accordance_characteristic_second_sequence_id_brin ON accordance_characteristic USING brin (second_chain_id);
CREATE INDEX ix_accordance_characteristic_sequences_ids_brin ON accordance_characteristic USING brin (first_chain_id, second_chain_id);
CREATE INDEX ix_accordance_characteristic_sequences_ids_characteristic_link_id_brin ON accordance_characteristic USING brin (first_chain_id, second_chain_id, characteristic_link_id);

CREATE INDEX ix_binary_characteristic_first_sequence_id_brin ON binary_characteristic USING brin (chain_id);
CREATE INDEX ix_binary_characteristic_sequence_id_characteristic_link_id_brin ON binary_characteristic USING brin (chain_id, characteristic_link_id);

-- 15.08.2017
-- Add indexes of new type to other tables.
CREATE INDEX ix_sequence_attribute_sequence_id_brin ON chain_attribute USING brin (chain_id);
CREATE INDEX ix_position_subsequence_id_brin ON "position" USING brin (subsequence_id);


-- 21.08.2017
-- Add additional result data column to task table.
-- Also fixing some naming errors.

ALTER TABLE task ADD COLUMN additional_result_data json;

COMMENT ON COLUMN chain.remote_db IS 'Remote db from which sequence is downloaded.';
ALTER TABLE fmotiv DROP CONSTRAINT chk_remote_id;
ALTER TABLE fmotiv RENAME remote_db_id TO remote_db;
COMMENT ON COLUMN fmotiv.remote_db IS 'Remote database from which sequence is downloaded.';
ALTER TABLE fmotiv ADD CONSTRAINT chk_remote_id CHECK (remote_db IS NULL AND remote_id IS NULL OR remote_db IS NOT NULL AND remote_id IS NOT NULL);

ALTER TABLE measure DROP CONSTRAINT chk_remote_id;
ALTER TABLE measure RENAME remote_db_id TO remote_db;
COMMENT ON COLUMN measure.remote_db IS 'Remote database from which sequence is downloaded.';
ALTER TABLE measure ADD CONSTRAINT chk_remote_id CHECK (remote_db IS NULL AND remote_id IS NULL OR remote_db IS NOT NULL AND remote_id IS NOT NULL);

-- 19.03.2018
-- Add table for sequences groups.

CREATE TABLE sequence_group
(
    id serial NOT NULL,
    name text COLLATE pg_catalog."default" NOT NULL,
    created timestamp with time zone NOT NULL,
    creator_id integer NOT NULL,
    modified timestamp with time zone NOT NULL,
    modifier_id integer NOT NULL,
    CONSTRAINT sequence_group_pkey PRIMARY KEY (id),
    CONSTRAINT uk_sequence_group_name UNIQUE (name)
);

COMMENT ON TABLE sequence_group IS 'Table storing information about sequences groups.';

CREATE TABLE sequence_group_matter
(
    group_id integer NOT NULL,
    matter_id bigint NOT NULL,
    PRIMARY KEY (matter_id, group_id),
    CONSTRAINT fk_sequence_group_matter FOREIGN KEY (matter_id)
        REFERENCES matter (id) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    CONSTRAINT fk_matter_sequence_group FOREIGN KEY (group_id)
        REFERENCES sequence_group (id) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

COMMENT ON TABLE sequence_group_matter IS 'Intermediate table for matters belonging to groups infromation.';

-- 28.03.2018
-- Add created/modified trigger on sequence_group.
-- And add references to users table.

CREATE TRIGGER tgiu_sequence_group_modified BEFORE INSERT OR UPDATE ON sequence_group FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();

COMMENT ON TRIGGER tgiu_sequence_group_modified ON sequence_group IS 'Тригер для вставки даты последнего изменения записи.';

ALTER TABLE sequence_group ALTER COLUMN created SET DEFAULT now();

ALTER TABLE sequence_group ADD CONSTRAINT fk_sequence_group_creator FOREIGN KEY (creator_id) REFERENCES dbo."AspNetUsers" ("Id") MATCH SIMPLE ON UPDATE CASCADE ON DELETE NO ACTION;

ALTER TABLE sequence_group ADD CONSTRAINT fk_sequence_group_modifier FOREIGN KEY (modifier_id) REFERENCES dbo."AspNetUsers" ("Id") MATCH SIMPLE ON UPDATE CASCADE ON DELETE NO ACTION;

-- 06.04.2018
-- Add new congeneric characteristics.

ALTER TABLE congeneric_characteristic_link DROP CONSTRAINT congeneric_characteristic_check;
ALTER TABLE congeneric_characteristic_link ADD CONSTRAINT congeneric_characteristic_check CHECK (congeneric_characteristic::integer <@ int4range(1, 24, '[]'::text));


INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (19,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (19,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (19,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (19,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (19,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (20,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (20,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (20,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (20,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (20,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (21,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (21,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (21,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (21,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (21,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (22,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (22,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (22,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (22,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (22,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (23,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (23,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (23,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (23,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (23,5);

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (24,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (24,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (24,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (24,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (24,5);

-- 11.04.2018
-- Drop redundant constraints.

ALTER TABLE congeneric_characteristic_link DROP CONSTRAINT congeneric_characteristic_check;
ALTER TABLE full_characteristic_link DROP CONSTRAINT full_characteristic_check;

INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (25,1);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (25,2);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (25,3);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (25,4);
INSERT INTO congeneric_characteristic_link (congeneric_characteristic, link) VALUES (25,5);

INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (55,1);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (55,2);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (55,3);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (55,4);
INSERT INTO full_characteristic_link (full_characteristic, link) VALUES (55,5);

-- 12.04.2018
-- Add nature to sequence_group table.

ALTER TABLE sequence_group ADD COLUMN nature smallint;
ALTER TABLE sequence_group ALTER COLUMN nature SET NOT NULL;
COMMENT ON COLUMN sequence_group.nature IS 'Nature of the objects in the group.';

ALTER TABLE full_characteristic_link ADD COLUMN arrangement_type smallint NOT NULL DEFAULT 0;
ALTER TABLE full_characteristic_link DROP CONSTRAINT uk_full_characteristic_link;
ALTER TABLE full_characteristic_link ADD CONSTRAINT uk_full_characteristic_link UNIQUE (full_characteristic, link, arrangement_type);

ALTER TABLE congeneric_characteristic_link ADD COLUMN arrangement_type smallint NOT NULL DEFAULT 0;
ALTER TABLE congeneric_characteristic_link DROP CONSTRAINT uk_congeneric_characteristic_link;
ALTER TABLE congeneric_characteristic_link ADD CONSTRAINT uk_congeneric_characteristic_link UNIQUE (congeneric_characteristic, link, arrangement_type);

-- 25.04.2018
-- Add sequence group column.

ALTER TABLE sequence_group ADD COLUMN sequence_group_type SMALLINT;

-- 17.02.2019
-- Change DB structure for fmotifs.

DROP TABLE fmotiv;

CREATE TABLE fmotif
(
  id bigint NOT NULL DEFAULT nextval('elements_id_seq'::regclass), 
  value character varying(255), 
  description text, 
  name character varying(255), 
  notation smallint NOT NULL DEFAULT 6, 
  created timestamp with time zone NOT NULL DEFAULT now(),
  modified timestamp with time zone NOT NULL DEFAULT now(),
  alphabet bigint[] NOT NULL, 
  building integer[] NOT NULL,
  fmotif_type smallint NOT NULL,
  CONSTRAINT pk_fmotif PRIMARY KEY (id),
  CONSTRAINT fk_fmotif_element_key FOREIGN KEY (id)
      REFERENCES element_key (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION DEFERRABLE INITIALLY DEFERRED
) INHERITS (element);

ALTER TABLE fmotif OWNER TO postgres;
COMMENT ON TABLE fmotif IS 'Fmotifs table';
COMMENT ON COLUMN fmotif.id IS 'Unique internal identificator';
COMMENT ON COLUMN fmotif.value IS 'Fmotif hash';
COMMENT ON COLUMN fmotif.description IS 'Fmotif description';
COMMENT ON COLUMN fmotif.name IS 'Fmotif name';
COMMENT ON COLUMN fmotif.notation IS 'Fmotif notation, always 6';
COMMENT ON COLUMN fmotif.created IS 'Creation date';
COMMENT ON COLUMN fmotif.alphabet IS 'Fmotif alphabet of notes';
COMMENT ON COLUMN fmotif.building IS 'Fmotif order';
COMMENT ON COLUMN fmotif.fmotif_type IS 'Type of fmotif';

CREATE INDEX ix_fmotif_alphabet ON fmotif USING gin (alphabet);

CREATE TRIGGER tgi_fmotif_building_check BEFORE INSERT ON fmotif FOR EACH ROW EXECUTE PROCEDURE trigger_building_check();
COMMENT ON TRIGGER tgi_fmotif_building_check ON fmotif IS 'Trigger validating fmotif order';

CREATE TRIGGER tgiu_fmotif_alphabet AFTER INSERT OR UPDATE OF alphabet ON fmotif FOR EACH STATEMENT EXECUTE PROCEDURE trigger_check_alphabet();
COMMENT ON TRIGGER tgiu_fmotif_alphabet ON fmotif IS 'Trigger validating fmotif alphabet';

CREATE TRIGGER tgiu_fmotif_modified BEFORE INSERT OR UPDATE ON fmotif FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();
COMMENT ON TRIGGER tgiu_fmotif_modified ON fmotif IS 'Trigger filling creation and modification date';

CREATE TRIGGER tgiud_fmotif_element_key_bound AFTER INSERT OR UPDATE OF id OR DELETE ON fmotif FOR EACH ROW EXECUTE PROCEDURE trigger_element_key_bound();
COMMENT ON TRIGGER tgiud_fmotif_element_key_bound ON fmotif IS 'Trigger that dublicates insert, update and delete of fmotifs into element_key table';
  
-- 19.02.2019
-- Change DB structure for measures.

ALTER TABLE measure DROP COLUMN matter_id;
ALTER TABLE measure DROP COLUMN remote_id;
ALTER TABLE measure DROP COLUMN remote_db;
ALTER TABLE measure DROP CONSTRAINT fk_measure_chain_key;
DROP TRIGGER tgiud_measure_chain_key_bound ON measure;

-- 01.03.2019
-- Recreate unique constraints on sequences tables.

ALTER TABLE dna_chain ADD CONSTRAINT uk_dna_chain UNIQUE (matter_id, notation);
ALTER TABLE literature_chain ADD CONSTRAINT uk_literature_chain UNIQUE (notation, matter_id, language, translator);
ALTER TABLE music_chain ADD CONSTRAINT uk_music_chain UNIQUE (matter_id, notation);
ALTER TABLE data_chain ADD CONSTRAINT uk_data_chain UNIQUE (notation, matter_id);

-- 06.03.2019
-- Change music_chain structure.
ALTER TABLE music_chain ADD COLUMN pause_treatment smallint NOT NULL DEFAULT 0;
ALTER TABLE music_chain ADD COLUMN sequential_transfer boolean NOT NULL DEFAULT false;
ALTER TABLE music_chain DROP CONSTRAINT uk_music_chain;
ALTER TABLE music_chain ADD CONSTRAINT uk_music_chain UNIQUE (matter_id, notation, pause_treatment, sequential_transfer);
ALTER TABLE music_chain ADD CONSTRAINT chk_pause_treatment_and_sequential_transfer CHECK ((notation = 6 AND pause_treatment != 0) OR ((notation = 7 OR notation = 8) AND pause_treatment = 0 AND NOT sequential_transfer));

-- 03.06.2019
-- Delete priority from notes.

ALTER TABLE note DROP COLUMN priority;

-- 23.11.2019
-- Replace plv8js procedures with pl/pgsql procedures.

CREATE OR REPLACE FUNCTION trigger_element_update_alphabet() RETURNS trigger
LANGUAGE 'plpgsql' VOLATILE AS
$BODY$
BEGIN
IF TG_OP = 'UPDATE' THEN
	UPDATE chain SET alphabet = c1.alphabet FROM (SELECT c1.id, array_replace(c1.alphabet, OLD.id, NEW.id) alphabet FROM chain c1 WHERE alphabet @> ARRAY[OLD.id]) c1 WHERE chain.id = c1.id;
	RETURN NEW;
END IF; 
	RAISE EXCEPTION 'Unknown operation. This trigger is only meat for update operations on tables with alphabet field';
END
$BODY$;
COMMENT ON FUNCTION trigger_element_update_alphabet() IS 'Automaticly updates elements ids in sequences alphabet when ids are changed in element table.';

CREATE OR REPLACE FUNCTION trigger_element_key_insert() RETURNS trigger
LANGUAGE 'plpgsql' VOLATILE AS
$BODY$
DECLARE
element_exists bool;
BEGIN
IF TG_OP = 'INSERT' THEN
	SELECT count(*) = 1 INTO element_exists FROM element WHERE id = NEW.id;
	IF element_exists THEN
		RETURN NEW;
	END IF;
	RAISE EXCEPTION 'Cannot add record into element_key before adding record into element table or its child.';
END IF;
RAISE EXCEPTION 'Unknown operation. This trigger only works on insert into table with id field.';
END
$BODY$;
COMMENT ON FUNCTION trigger_element_key_insert() IS 'Adds new element id into element_key table.';

CREATE OR REPLACE FUNCTION trigger_element_key_bound() RETURNS trigger
LANGUAGE 'plpgsql' VOLATILE AS
$BODY$
BEGIN
IF TG_OP = 'INSERT' THEN
	INSERT INTO element_key VALUES (NEW.id);
	return NEW;
ELSE IF TG_OP = 'UPDATE' AND NEW.id != OLD.id THEN
	UPDATE element_key SET id = NEW.id WHERE id = OLD.id;
	return NEW;
ELSE IF TG_OP = 'DELETE' THEN
	DELETE FROM element_key WHERE id = OLD.id;
	return OLD;
END IF;
END IF;
END IF;
END
$BODY$;
COMMENT ON FUNCTION trigger_element_key_bound() IS 'Links insert, update and delete actions on element tables with element_key table.';

CREATE OR REPLACE FUNCTION trigger_element_delete_alphabet_bound() RETURNS trigger
LANGUAGE 'plpgsql' VOLATILE AS 
$BODY$
DECLARE
element_used bool;
BEGIN
IF TG_OP = 'DELETE' THEN
	SELECT count(*) > 0 INTO element_used FROM (SELECT DISTINCT unnest(alphabet) a FROM chain) c WHERE c.a = OLD.id;
	IF element_used THEN
		return OLD;
	ELSE
		RAISE EXCEPTION  'Cannot delete element, because it still is in some of the cequences alphabets.';
	END IF;
	
ELSE
	RAISE EXCEPTION  'Unknown operation. This trigger shoud be used only in delete operation on tables with id field.';
END IF;
END
$BODY$;
COMMENT ON FUNCTION trigger_element_delete_alphabet_bound() IS 'Checks if there is still seqiences with element to be deleted, and if there are such sequences it raises exception.';

CREATE OR REPLACE FUNCTION trigger_delete_chain_characteristics() RETURNS trigger
LANGUAGE 'plpgsql' VOLATILE AS 
$BODY$
BEGIN
IF TG_OP = 'UPDATE' THEN
	DELETE FROM full_characteristic WHERE full_characteristic.chain_id = OLD.id;
	DELETE FROM binary_characteristic WHERE binary_characteristic.chain_id = OLD.id;
	DELETE FROM congeneric_characteristic WHERE congeneric_characteristic.chain_id = OLD.id;
	DELETE FROM accordance_characteristic WHERE accordance_characteristic.first_chain_id = OLD.id OR accordance_characteristic.second_chain_id = OLD.id;
	RETURN NEW;
ELSE
	RAISE EXCEPTION 'Unknown operation. This trigger only works on UPDATE operation.';
END IF;
END;
$BODY$;

CREATE OR REPLACE FUNCTION trigger_check_elements_in_alphabets() RETURNS trigger
LANGUAGE 'plpgsql' VOLATILE AS
$BODY$
DECLARE
first_element_in_alphabet bool;
second_element_in_alphabet bool;
BEGIN
IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
	first_element_in_alphabet := check_element_in_alphabet(NEW.first_chain_id, NEW.first_element_id);
	second_element_in_alphabet := check_element_in_alphabet(NEW.second_chain_id, NEW.second_element_id);
	IF first_element_in_alphabet AND second_element_in_alphabet THEN
		RETURN NEW;
	ELSE 
		RAISE EXCEPTION 'New characteristic is referencing element not present in sequence alphabet.';
	END IF;
ELSE
	RAISE EXCEPTION 'Unknown operation. This trigger shoud be used only in insert and update operation on tables with first_chain_id, second_chain_id, first_element_id, second_element_id.';
END IF;
END
$BODY$;
COMMENT ON FUNCTION trigger_check_elements_in_alphabets() IS 'Checks if elements of accordance characteristics are present in alphabets of corresponding sequences. Essentialy this function serves as foregin key referencing alphabet of sequence.';

CREATE OR REPLACE FUNCTION trigger_check_elements_in_alphabet() RETURNS trigger
LANGUAGE 'plpgsql' VOLATILE AS
$BODY$
DECLARE
first_element_in_alphabet bool;
second_element_in_alphabet bool;
BEGIN
IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
	first_element_in_alphabet := check_element_in_alphabet(NEW.chain_id, NEW.first_element_id);
	second_element_in_alphabet := check_element_in_alphabet(NEW.chain_id, NEW.second_element_id);
	IF first_element_in_alphabet AND second_element_in_alphabet THEN
		RETURN NEW;
	ELSE 
		RAISE EXCEPTION 'New characteristic is referencing element or elements (first_id = %, second_id = %) not present in sequence (id = %) alphabet.', NEW.first_element_id, NEW.second_element_id, NEW.chain_id;
	END IF;
ELSE
	RAISE EXCEPTION 'Unknown operation. This trigger shoud be used only in insert and update operation on tables with chain_id, first_element_id, second_element_id.';
END IF;
END
$BODY$;
COMMENT ON FUNCTION trigger_check_elements_in_alphabet() IS 'Checks if elements of binary characteristic are present in alphabet of corresponding sequence. Essentialy this function serves as foregin key referencing alphabet of sequence.';

CREATE OR REPLACE FUNCTION trigger_check_element_in_alphabet() RETURNS trigger
LANGUAGE 'plpgsql' VOLATILE AS
$BODY$
DECLARE
element_in_alphabet bool;
BEGIN
IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
	element_in_alphabet := check_element_in_alphabet(NEW.chain_id, NEW.element_id);
	IF element_in_alphabet THEN
		RETURN NEW;
	ELSE 
		RAISE EXCEPTION 'New characteristic is referencing element (id = %) not present in sequence (id = %) alphabet.', NEW.element_id, NEW.chain_id;
	END IF;
ELSE
	RAISE EXCEPTION 'Unknown operation. This trigger shoud be used only in insert and update operation on tables with chain_id, element_id.';
END IF;
END
$BODY$;
COMMENT ON FUNCTION trigger_check_element_in_alphabet() IS 'Checks if element of congeneric characteristic is present in alphabet of corresponding sequence. Essentialy this function serves as foregin key referencing alphabet of sequence.';

CREATE OR REPLACE FUNCTION trigger_chain_key_insert() RETURNS trigger
LANGUAGE 'plpgsql' VOLATILE AS
$BODY$
DECLARE
sequence_with_id_count integer;
BEGIN
IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
	SELECT count(*) INTO sequence_with_id_count FROM(SELECT id FROM chain WHERE id = NEW.id UNION ALL SELECT id FROM subsequence WHERE id = NEW.id) s;
	IF sequence_with_id_count = 1 THEN
		RETURN NEW;
	ELSE IF sequence_with_id_count = 0 THEN
		RAISE EXCEPTION 'New record in table chain_key cannot be addded because there is no sequences with given id.';
	END IF;
		RAISE EXCEPTION 'New record in table chain_key cannot be addded because there more than one sequences with given id.';
	END IF;
ELSE	
	RAISE EXCEPTION 'Unknown operation. This trigger only operates on INSERT operation on tables with id column.';
END IF;
END
$BODY$;
COMMENT ON FUNCTION trigger_chain_key_insert() IS 'Checks that there is one and only one sequence with the given id.';

CREATE OR REPLACE FUNCTION trigger_chain_key_bound() RETURNS trigger
LANGUAGE 'plpgsql' VOLATILE AS
$BODY$
BEGIN
IF TG_OP = 'INSERT' THEN
	INSERT INTO chain_key VALUES (NEW.id);
	return NEW;
ELSE IF TG_OP = 'UPDATE' AND NEW.id != OLD.id THEN
	UPDATE chain_key SET id = NEW.id WHERE id = OLD.id;
	return NEW;
ELSE IF TG_OP = 'DELETE' THEN
	DELETE FROM chain_key WHERE id = OLD.id;
	return OLD;
END IF;
END IF;
END IF;
END
$BODY$;
COMMENT ON FUNCTION trigger_chain_key_bound() IS 'Links insert, update and delete operations on sequences tables with chain_key table.';

CREATE OR REPLACE FUNCTION trigger_building_check() RETURNS trigger
LANGUAGE 'plpgsql' VOLATILE AS
$BODY$
DECLARE
max_value integer;
BEGIN
IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
	IF NEW.building[1] != 1 THEN
		RAISE EXCEPTION  'First order value is not 1. Actual value %.', NEW.building[1];
	END IF;
	max_value := 0;
	FOR i IN array_lower(NEW.building, 1)..array_upper(NEW.building, 1) LOOP
		IF NEW.building[i] > (max_value + 1) THEN
			RAISE EXCEPTION  'Order is incorrect starting from % position.', i ;
		END IF;
		IF NEW.building[i] = (max_value + 1) THEN
			max_value := NEW.building[i];
		END IF;
	END LOOP;
	IF max_value != array_length(NEW.alphabet, 1) THEN
		RAISE EXCEPTION  'Alphabet size is not equal to the order maximum value. Alphabet elements count %, and order max value %.', array_length(NEW.alphabet, 1), max_value;
	END IF;
	RETURN NEW;
ELSE
	RAISE EXCEPTION  'Unknown operation. This trigger only operates on INSERT operation on tables with building column.';
END IF;
END
$BODY$;
COMMENT ON FUNCTION trigger_building_check() IS 'Validates inner consistency of the order of given sequence.';

CREATE OR REPLACE FUNCTION trigger_set_modified() RETURNS trigger
LANGUAGE 'plpgsql'
VOLATILE AS
$BODY$
BEGIN
NEW.modified := now();
IF TG_OP = 'INSERT' THEN
	NEW.created := now();
	RETURN NEW;
END IF;
IF TG_OP = 'UPDATE' THEN
	NEW.created := OLD.created;
	RETURN NEW;
END IF;
	RAISE EXCEPTION 'Unknown operation. This trigger only operates on INSERT and UPDATE operation on tables with modified and created columns.';
END;
$BODY$;
COMMENT ON FUNCTION trigger_set_modified() IS 'Rewrites created and modified columns with current values.';

CREATE OR REPLACE FUNCTION check_element_in_alphabet(chain_id bigint,element_id bigint) RETURNS boolean
LANGUAGE 'plpgsql' VOLATILE
PARALLEL UNSAFE AS
$BODY$
BEGIN
RETURN (SELECT count(*) = 1 FROM (SELECT unnest(alphabet) a FROM chain WHERE id = chain_id) c WHERE c.a = element_id);
END
$BODY$;
COMMENT ON FUNCTION check_element_in_alphabet(bigint, bigint) IS 'Checks if element with given id is present in alphabet of given sequence.';

CREATE OR REPLACE FUNCTION db_integrity_test() RETURNS void
LANGUAGE 'plpgsql' VOLATILE
PARALLEL UNSAFE AS
$BODY$
DECLARE
sequence_ids_count bigint;
distincs_sequence_ids_count bigint;
sequences_keys_disproportion integer;
elements_ids_count integer;
distinct_elements_count integer;
element_key_disproportion integer;
orphaned_elements_count integer;
orphaned_congeneric_characteristics integer;
orphaned_binary_characteristics integer;
orphaned_accordance_characteristics integer;
BEGIN
RAISE INFO 'Checking referential integrity of the database.';

RAISE INFO 'Checking "sequence" table and its children.';
SELECT COUNT(s.id) INTO sequence_ids_count FROM (SELECT id FROM chain UNION ALL SELECT id FROM subsequence) s;
SELECT COUNT(DISTINCT s.id) INTO distincs_sequence_ids_count FROM (SELECT id FROM chain UNION SELECT id FROM subsequence) s;
IF sequence_ids_count != distincs_sequence_ids_count THEN
	RAISE EXCEPTION  'Ids in "sequence" table and/or its cildren are not unique.';
ELSE
	RAISE INFO 'All sequence ids are unique.';
END IF;

RAISE INFO 'Checking accordance of records in "sequence" table and its children to the records in sequence_key table.';
SELECT COUNT(*) INTO sequences_keys_disproportion 
	FROM (SELECT id FROM chain UNION ALL SELECT id FROM subsequence) s 
	FULL OUTER JOIN chain_key sk ON sk.id = s.id 
	WHERE s.id IS NULL OR sk.id IS NULL;
IF sequences_keys_disproportion > 0 THEN
	RAISE EXCEPTION 'Number of records in sequence_key is not equal to number of records in sequence table and its children.';
ELSE
	RAISE INFO 'sequence_key is in sync with sequence table and its children.';
END IF;

RAISE INFO 'Sequences tables are all checked.';

RAISE INFO 'Checking "element" table and its children.';
SELECT COUNT(id) INTO elements_ids_count FROM element;
SELECT COUNT(DISTINCT id) INTO distinct_elements_count FROM element;
IF elements_ids_count != distinct_elements_count THEN
	RAISE EXCEPTION 'ids in "element" table and/or its cildren are not unique.';
ELSE
	RAISE INFO 'All element ids are unique.';
END IF;

RAISE INFO 'Checking accordance of records in "element" table and its children to the records in element_key table.';
SELECT COUNT (*) INTO element_key_disproportion 
	FROM (SELECT e.id, ek.id  FROM element e 
		  FULL OUTER JOIN element_key ek 
		  ON ek.id = e.id 
		  WHERE e.id IS NULL OR ek.id IS NULL) ec;
IF element_key_disproportion > 0 THEN
	RAISE EXCEPTION 'Number of records in element_key is not equal to number of records in element and its children.';
ELSE
	RAISE INFO 'element_key is in sync with element and its children.';
END IF;

RAISE INFO 'Elements tables are all checked.';

RAISE INFO 'Checking alphabets of all sequences.';
SELECT COUNT(c.a) INTO orphaned_elements_count 
	FROM (SELECT DISTINCT unnest(alphabet) a FROM chain) c 
		  LEFT OUTER JOIN element_key e 
		  ON e.id = c.a 
		  WHERE e.id IS NULL;
IF orphaned_elements_count > 0 THEN 
	RAISE EXCEPTION 'There are % missing elements of alphabet.', orphaned_elements_count;
ELSE
	RAISE INFO 'All alphabets elements are present in element_key table.';
END IF;

SELECT COUNT(cc.id) INTO orphaned_congeneric_characteristics
	FROM congeneric_characteristic cc 
	LEFT OUTER JOIN
		(SELECT unnest(alphabet) a, id FROM chain) c 
	ON c.id = cc.chain_id AND c.a = cc.element_id 
	WHERE c.a IS NULL;
IF orphaned_congeneric_characteristics > 0 THEN
	RAISE EXCEPTION 'There are % orphaned congeneric characteristics without according elements in alphabets.', orphaned_congeneric_characteristics;
ELSE
	RAISE INFO 'All congeneric characteristics have corresponding elements in alphabets.';
END IF;

SELECT COUNT(bc.id) INTO orphaned_binary_characteristics
	FROM binary_characteristic bc 
	LEFT OUTER JOIN 
		(SELECT unnest(alphabet) a, id FROM chain) c 
	ON c.id = bc.chain_id AND (c.a = bc.first_element_id OR c.a = bc.second_element_id)
	WHERE c.a IS NULL;
IF orphaned_binary_characteristics > 0 THEN
	RAISE EXCEPTION 'There are % orphaned binary characteristics without according elements in alphabets.', orphaned_binary_characteristics;
ELSE
	RAISE INFO 'All binary characteristics have corresponding elements in alphabets.';
END IF;

SELECT COUNT(ac.id) INTO orphaned_accordance_characteristics
	FROM accordance_characteristic ac 
	LEFT OUTER JOIN 
		(SELECT unnest(alphabet) a, id FROM chain) c 
	ON (c.id = ac.first_chain_id AND c.a = ac.first_element_id) OR(c.id = ac.second_chain_id AND c.a = ac.second_element_id)
	WHERE c.a IS NULL;
IF orphaned_accordance_characteristics > 0 THEN
	RAISE EXCEPTION 'There are % orphaned accordance characteristics without according elements in alphabets.', orphaned_accordance_characteristics;
ELSE
	RAISE INFO 'All accordance characteristics have corresponding elements in alphabets.';
END IF;

RAISE INFO 'All alphabets are checked.';

RAISE INFO 'Referential integrity of database is successfully checked.';
END
$BODY$;
COMMENT ON FUNCTION db_integrity_test() IS 'Procedure for cheking referential integrity of the database.';

-- 07.12.2019
-- Add multi-sequence table and reference columns in mater table.

CREATE TABLE multisequence
(
    id serial NOT NULL,
    name text NOT NULL,
    nature smallint NOT NULL DEFAULT 1,
    PRIMARY KEY (id),
    CONSTRAINT uk_multisequence_name UNIQUE (name)

);

COMMENT ON TABLE matter IS 'Table for all research objects, samples, texts, etc.';
ALTER TABLE matter ADD COLUMN multisequence_id integer;
ALTER TABLE matter ADD COLUMN multisequence_number smallint;
ALTER TABLE matter ADD CONSTRAINT fk_matter_multisequence FOREIGN KEY (multisequence_id) REFERENCES multisequence (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE SET NULL;
ALTER TABLE matter ADD CONSTRAINT chk_multisequence_reference CHECK ((multisequence_id IS NULL AND multisequence_number IS NULL) OR (multisequence_id IS NOT NULL AND multisequence_number IS NOT NULL));

-- 14.12.2019
-- Update and fix trigger procedures.

CREATE OR REPLACE FUNCTION trigger_check_alphabet() RETURNS trigger
LANGUAGE 'plpgsql'
VOLATILE AS
$BODY$
DECLARE
	orphaned_elements integer;
	alphabet_elemnts_not_unique bool;
BEGIN
	IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
		SELECT a.dc != a.ec INTO alphabet_elemnts_not_unique FROM (SELECT COUNT(DISTINCT e) dc, COUNT(e) ec FROM unnest(NEW.alphabet) e) a;
		IF alphabet_elemnts_not_unique THEN
			RAISE EXCEPTION 'Alphabet elements are not unique. Alphabet %', NEW.alphabet ;
		END IF;
		
		SELECT count(1) INTO orphaned_elements result FROM unnest(NEW.alphabet) a LEFT OUTER JOIN element_key e ON e.id = a WHERE e.id IS NULL;
		IF orphaned_elements != 0 THEN 
			RAISE EXCEPTION 'There are % elements of the alphabet missing in database.', orphaned_elements;
		END IF;
		
		RETURN NEW;
	END IF;
    RAISE EXCEPTION 'Unknown operation. This trigger only operates on INSERT and UPDATE operation on tables with alphabet column (of array type).';
END;
$BODY$;
COMMENT ON FUNCTION trigger_check_alphabet() IS 'Trigger function checking that all alphabet elements of the sequencs are in database.';

DROP TRIGGER tgiu_chain_alphabet ON chain;
CREATE TRIGGER tgiu_chain_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON chain FOR EACH ROW EXECUTE PROCEDURE trigger_check_alphabet();
COMMENT ON TRIGGER tgiu_chain_alphabet_check ON chain IS 'Checks that all alphabet elements are present in database.';

DROP TRIGGER tgiu_data_chain_alphabet ON data_chain;
CREATE TRIGGER tgiu_data_chain_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON data_chain FOR EACH ROW EXECUTE PROCEDURE trigger_check_alphabet();
COMMENT ON TRIGGER tgiu_data_chain_alphabet_check ON data_chain IS 'Checks that all alphabet elements are present in database.';

DROP TRIGGER tgiu_dna_chain_alphabet ON dna_chain;
CREATE TRIGGER tgiu_dna_chain_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON dna_chain FOR EACH ROW EXECUTE PROCEDURE trigger_check_alphabet();
COMMENT ON TRIGGER tgiu_dna_chain_alphabet_check ON dna_chain IS 'Checks that all alphabet elements are present in database.';

DROP TRIGGER tgiu_fmotif_alphabet ON fmotif;
CREATE TRIGGER tgiu_fmotif_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON fmotif FOR EACH ROW EXECUTE PROCEDURE trigger_check_alphabet();
COMMENT ON TRIGGER tgiu_fmotif_alphabet_check ON fmotif IS 'Checks that all alphabet elements are present in database.';

DROP TRIGGER tgiu_literature_chain_alphabet ON literature_chain;
CREATE TRIGGER tgiu_literature_chain_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON literature_chain FOR EACH ROW EXECUTE PROCEDURE trigger_check_alphabet();
COMMENT ON TRIGGER tgiu_literature_chain_alphabet_check ON literature_chain IS 'Checks that all alphabet elements are present in database.';

DROP TRIGGER tgiu_measure_alphabet ON measure;
CREATE TRIGGER tgiu_measure_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON measure FOR EACH ROW EXECUTE PROCEDURE trigger_check_alphabet();
COMMENT ON TRIGGER tgiu_measure_alphabet_check ON measure IS 'Checks that all alphabet elements are present in database.';

DROP TRIGGER tgiu_music_chain_alphabet ON music_chain;
CREATE TRIGGER tgiu_music_chain_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON music_chain FOR EACH ROW EXECUTE PROCEDURE trigger_check_alphabet();
COMMENT ON TRIGGER tgiu_music_chain_alphabet_check ON music_chain IS 'Checks that all alphabet elements are present in database.';

CREATE OR REPLACE FUNCTION trigger_element_delete_alphabet_bound() RETURNS trigger
LANGUAGE 'plpgsql'
VOLATILE AS
$BODY$
DECLARE
element_used bool;
BEGIN
IF TG_OP = 'DELETE' THEN
	SELECT count(*) > 0 INTO element_used 
	FROM (SELECT DISTINCT unnest(alphabet) a 
		  FROM (SELECT alphabet FROM chain 
				UNION SELECT alphabet FROM fmotif 
				UNION SELECT alphabet FROM measure) c
		  WHERE c.alphabet @> ARRAY[OLD.id]) s;
	IF element_used THEN
		RAISE EXCEPTION 'Cannot delete element, because it still is in some of the sequences alphabets.';
	ELSE
		return OLD;
	END IF;
END IF;
RAISE EXCEPTION 'Unknown operation. This trigger shoud be used only in delete operation on tables with id field.';
END
$BODY$;
COMMENT ON FUNCTION trigger_element_delete_alphabet_bound() IS 'Checks if there is still sequences with element to be deleted, and if there are such sequences it raises exception.';

CREATE OR REPLACE FUNCTION trigger_element_update_alphabet() RETURNS trigger
LANGUAGE 'plpgsql'
VOLATILE AS
$BODY$
BEGIN
IF TG_OP = 'UPDATE' THEN
	UPDATE chain SET alphabet = c1.alphabet FROM (SELECT c1.id, array_replace(c1.alphabet, OLD.id, NEW.id) alphabet FROM chain c1 WHERE alphabet @> ARRAY[OLD.id]) c1 WHERE chain.id = c1.id;
	UPDATE fmotif SET alphabet = c1.alphabet FROM (SELECT c1.id, array_replace(c1.alphabet, OLD.id, NEW.id) alphabet FROM fmotif c1 WHERE alphabet @> ARRAY[OLD.id]) c1 WHERE fmotif.id = c1.id;
	UPDATE measure SET alphabet = c1.alphabet FROM (SELECT c1.id, array_replace(c1.alphabet, OLD.id, NEW.id) alphabet FROM measure c1 WHERE alphabet @> ARRAY[OLD.id]) c1 WHERE measure.id = c1.id;
	
	RETURN NEW;
END IF; 
RAISE EXCEPTION 'Unknown operation. This trigger is only meat for update operations on tables with alphabet field';
END
$BODY$;
COMMENT ON FUNCTION trigger_element_update_alphabet() IS 'Automaticly updates elements ids in sequences alphabet when ids are changed in element table.';

DROP TRIGGER tgu_chain_characteristics ON chain;
CREATE TRIGGER tgu_chain_characteristics_delete AFTER UPDATE OF alphabet, building ON chain FOR EACH ROW EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_chain_characteristics_delete ON chain IS 'Trigger deleting all characteristics of sequences that has been updated.';

DROP TRIGGER tgu_data_chain_characteristics ON data_chain;
CREATE TRIGGER tgu_data_chain_characteristics_delete AFTER UPDATE OF alphabet, building ON data_chain FOR EACH ROW EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_data_chain_characteristics_delete ON data_chain IS 'Trigger deleting all characteristics of sequences that has been updated.';

DROP TRIGGER tgu_dna_chain_characteristics ON dna_chain;
CREATE TRIGGER tgu_dna_chain_characteristics_delete AFTER UPDATE OF alphabet, building ON dna_chain FOR EACH ROW EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_dna_chain_characteristics_delete ON dna_chain IS 'Trigger deleting all characteristics of sequences that has been updated.';

DROP TRIGGER tgu_literature_chain_characteristics ON literature_chain;
CREATE TRIGGER tgu_literature_chain_characteristics_delete AFTER UPDATE OF alphabet, building ON literature_chain FOR EACH ROW EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_literature_chain_characteristics_delete ON literature_chain IS 'Trigger deleting all characteristics of sequences that has been updated.';

DROP TRIGGER tgu_music_chain_characteristics ON music_chain;
CREATE TRIGGER tgu_music_chain_characteristics_delete AFTER UPDATE OF alphabet, building ON music_chain FOR EACH ROW EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_music_chain_characteristics_delete ON music_chain IS 'Trigger deleting all characteristics of sequences that has been updated.';

DROP TRIGGER tgu_subsequence_characteristics ON subsequence;
CREATE TRIGGER tgu_subsequence_characteristics_delete AFTER UPDATE OF start, length, chain_id ON subsequence FOR EACH ROW EXECUTE PROCEDURE trigger_delete_chain_characteristics();
COMMENT ON TRIGGER tgu_subsequence_characteristics_delete ON subsequence IS 'Trigger deleting all characteristics of subsequences that has been updated.';

DROP TRIGGER tgi_chain_building_check ON chain;
CREATE TRIGGER tgiu_chain_building_check  BEFORE INSERT OR UPDATE OF alphabet, building ON chain FOR EACH ROW EXECUTE PROCEDURE trigger_building_check();
COMMENT ON TRIGGER tgiu_chain_building_check ON chain IS 'Validates order of the sequence and checks its consistency with the alphabet.';

DROP TRIGGER tgi_data_chain_building_check ON data_chain;
CREATE TRIGGER tgiu_data_chain_building_check  BEFORE INSERT OR UPDATE OF alphabet, building ON data_chain FOR EACH ROW EXECUTE PROCEDURE trigger_building_check();
COMMENT ON TRIGGER tgiu_data_chain_building_check ON data_chain IS 'Validates order of the sequence and checks its consistency with the alphabet.';

DROP TRIGGER tgi_dna_chain_building_check ON dna_chain;
CREATE TRIGGER tgiu_dna_chain_building_check  BEFORE INSERT OR UPDATE OF alphabet, building ON dna_chain FOR EACH ROW EXECUTE PROCEDURE trigger_building_check();
COMMENT ON TRIGGER tgiu_dna_chain_building_check ON dna_chain IS 'Validates order of the sequence and checks its consistency with the alphabet.';

DROP TRIGGER tgi_fmotif_building_check ON fmotif;
CREATE TRIGGER tgiu_fmotif_building_check  BEFORE INSERT OR UPDATE OF alphabet, building ON fmotif FOR EACH ROW EXECUTE PROCEDURE trigger_building_check();
COMMENT ON TRIGGER tgiu_fmotif_building_check ON fmotif IS 'Validates order of the sequence and checks its consistency with the alphabet.';

DROP TRIGGER tgi_literature_chain_building_check ON literature_chain;
CREATE TRIGGER tgiu_literature_chain_building_check  BEFORE INSERT OR UPDATE OF alphabet, building ON literature_chain FOR EACH ROW EXECUTE PROCEDURE trigger_building_check();
COMMENT ON TRIGGER tgiu_literature_chain_building_check ON literature_chain IS 'Validates order of the sequence and checks its consistency with the alphabet.';

DROP TRIGGER tgi_measure_building_check ON measure;
CREATE TRIGGER tgiu_measure_building_check  BEFORE INSERT OR UPDATE OF alphabet, building ON measure FOR EACH ROW EXECUTE PROCEDURE trigger_building_check();
COMMENT ON TRIGGER tgiu_measure_building_check ON measure IS 'Validates order of the sequence and checks its consistency with the alphabet.';

DROP TRIGGER tgi_music_chain_building_check ON music_chain;
CREATE TRIGGER tgiu_music_chain_building_check  BEFORE INSERT OR UPDATE OF alphabet, building ON music_chain FOR EACH ROW EXECUTE PROCEDURE trigger_building_check();
COMMENT ON TRIGGER tgiu_music_chain_building_check ON music_chain IS 'Validates order of the sequence and checks its consistency with the alphabet.';

CREATE OR REPLACE FUNCTION check_element_in_alphabet(chain_id bigint,element_id bigint) RETURNS boolean
LANGUAGE 'plpgsql'
VOLATILE PARALLEL UNSAFE AS 
$BODY$
BEGIN
RETURN (SELECT count(*) = 1 
		FROM (SELECT alphabet a 
			  FROM chain 
			  WHERE id = chain_id) c 
		WHERE c.a @> ARRAY[element_id]);
END
$BODY$;

CREATE OR REPLACE FUNCTION trigger_element_key_insert() RETURNS trigger
LANGUAGE 'plpgsql'
VOLATILE AS
$BODY$
DECLARE
elements_with_id_count integer;
BEGIN
IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
	SELECT count(*) INTO elements_with_id_count FROM element WHERE id = NEW.id;
	IF elements_with_id_count = 1 THEN
		RETURN NEW;
	ELSE IF elements_with_id_count = 0 THEN
		RAISE EXCEPTION 'New record in table element_key cannot be addded because there is no elements with given id = %.', NEW.id;
	END IF;
		RAISE EXCEPTION 'New record in table element_key cannot be addded because there is more than one element with given id = %.', NEW.id;
	END IF;
	RAISE EXCEPTION 'Cannot add record into element_key before adding record into element table or its child.';
END IF;
RAISE EXCEPTION 'Unknown operation. This trigger only works on insert into table with id field.';
END
$BODY$;

ALTER FUNCTION trigger_chain_key_insert() RENAME TO trigger_chain_key_unique_check;
CREATE OR REPLACE FUNCTION trigger_chain_key_unique_check() RETURNS trigger
LANGUAGE 'plpgsql'
VOLATILE AS 
$BODY$
DECLARE
sequence_with_id_count integer;
BEGIN
IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
	SELECT count(*) INTO sequence_with_id_count FROM(SELECT id FROM chain WHERE id = NEW.id UNION ALL SELECT id FROM subsequence WHERE id = NEW.id) s;
	IF sequence_with_id_count = 1 THEN
		RETURN NEW;
	ELSE IF sequence_with_id_count = 0 THEN
		RAISE EXCEPTION 'New record in table chain_key cannot be addded because there is no sequences with given id.';
	END IF;
		RAISE EXCEPTION 'New record in table chain_key cannot be addded because there more than one sequences with given id. Sequences count = %', sequence_with_id_count;
	END IF;
ELSE	
	RAISE EXCEPTION 'Unknown operation. This trigger only operates on INSERT operation on tables with id column.';
END IF;
END
$BODY$;

CREATE FUNCTION trigger_check_notes_alphabet() RETURNS trigger
LANGUAGE 'plpgsql'
VOLATILE NOT LEAKPROOF AS
$BODY$
DECLARE
	orphaned_elements integer;
	alphabet_elemnts_not_unique bool;
BEGIN
	IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
		SELECT a.dc != a.ec INTO alphabet_elemnts_not_unique FROM (SELECT COUNT(DISTINCT e) dc, COUNT(e) ec FROM unnest(NEW.alphabet) e) a;
		IF alphabet_elemnts_not_unique THEN
			RAISE EXCEPTION 'Alphabet notes are not unique. Alphabet %', NEW.alphabet ;
		END IF;
		
		SELECT count(1) INTO orphaned_elements result FROM unnest(NEW.alphabet) a LEFT OUTER JOIN note e ON e.id = a WHERE e.id IS NULL;
		IF orphaned_elements != 0 THEN 
			RAISE EXCEPTION 'There are % notes of the alphabet missing in database.', orphaned_elements;
		END IF;
		
		RETURN NEW;
	END IF;
    RAISE EXCEPTION 'Unknown operation. This trigger only operates on INSERT and UPDATE operation on tables with alphabet column (of array type).';
END;
$BODY$;
COMMENT ON FUNCTION trigger_check_notes_alphabet() IS 'Trigger function checking that all alphabet notes of the music sequences are in database.';

DROP TRIGGER tgiu_measure_alphabet_check ON measure;
CREATE TRIGGER tgiu_measure_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON measure FOR EACH ROW EXECUTE PROCEDURE trigger_check_notes_alphabet();
COMMENT ON TRIGGER tgiu_measure_alphabet_check ON measure IS 'Checks that all alphabet elements are present in database.';

DROP TRIGGER tgiu_fmotif_alphabet_check ON fmotif;
CREATE TRIGGER tgiu_fmotif_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON fmotif FOR EACH ROW EXECUTE PROCEDURE trigger_check_notes_alphabet();
COMMENT ON TRIGGER tgiu_fmotif_alphabet_check ON fmotif IS 'Checks that all alphabet elements are present in database.';

-- 22.02.2020
-- Add task results table.

CREATE TABLE task_result
(
    id bigserial NOT NULL,
    task_id bigint NOT NULL,
    key text NOT NULL,
    value json NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT uk_task_result UNIQUE (task_id, key),
    CONSTRAINT fk_task_result_task FOREIGN KEY (task_id)
        REFERENCES task (id) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

COMMENT ON TABLE task_result IS 'Table with JSON results of tasks calculation. Results are stored as key/value pairs.';

-- 11.03.2020
-- Add images table.

ALTER TABLE matter ADD COLUMN source bytea;

CREATE TABLE image_sequence
(
    id bigint NOT NULL DEFAULT nextval('elements_id_seq'::regclass),
    notation smallint NOT NULL,
    order_extractor smallint NOT NULL,
	image_transformations smallint[] NOT NULL,
	matrix_transformations smallint[] NOT NULL,
    matter_id bigint NOT NULL,
    remote_id text,
    remote_db smallint,
	created timestamp with time zone NOT NULL DEFAULT now(),
    modified timestamp with time zone NOT NULL DEFAULT now(),
    CONSTRAINT pk_image_sequence PRIMARY KEY (id),
    CONSTRAINT fk_image_sequence_chain_key FOREIGN KEY (id)
        REFERENCES chain_key (id) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE NO ACTION
        DEFERRABLE INITIALLY DEFERRED,
    CONSTRAINT fk_image_sequence_matter FOREIGN KEY (matter_id)
        REFERENCES matter (id) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE NO ACTION,
    CONSTRAINT chk_remote_id CHECK (remote_db IS NULL AND remote_id IS NULL OR remote_db IS NOT NULL AND remote_id IS NOT NULL)
);
COMMENT ON TABLE image_sequence IS 'Table with information on image transformations and order extraction. Does not store an actual order of image and used for reference by characteristics tables.';
CREATE INDEX ix_image_sequence_matter_id  ON image_sequence USING btree (matter_id);
CREATE TRIGGER tgiu_image_sequence_modified  BEFORE INSERT OR UPDATE ON image_sequence FOR EACH ROW EXECUTE PROCEDURE trigger_set_modified();
CREATE TRIGGER tgiud_image_sequence_chain_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON image_sequence FOR EACH ROW EXECUTE PROCEDURE trigger_chain_key_bound();

-- 15.03.2020
-- Remove redundant columns in note and measure tables.

ALTER TABLE note DROP COLUMN onumerator;
ALTER TABLE note DROP COLUMN odenominator;

-- 17.05.2020
-- Fix chain_key check trigger.

CREATE OR REPLACE FUNCTION trigger_chain_key_unique_check()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
DECLARE
sequence_with_id_count integer;
BEGIN
IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
	SELECT count(*) INTO sequence_with_id_count FROM(
		SELECT id FROM chain WHERE id = NEW.id 
		UNION ALL 
		SELECT id FROM subsequence WHERE id = NEW.id
		UNION ALL 
		SELECT id FROM image_sequence WHERE id = NEW.id) s;
	IF sequence_with_id_count = 1 THEN
		RETURN NEW;
	ELSE IF sequence_with_id_count = 0 THEN
		RAISE EXCEPTION 'New record in table chain_key cannot be addded because there is no sequences with given id.';
	END IF;
		RAISE EXCEPTION 'New record in table chain_key cannot be addded because there more than one sequences with given id. Sequences count = %', sequence_with_id_count;
	END IF;
ELSE	
	RAISE EXCEPTION 'Unknown operation. This trigger only operates on INSERT operation on tables with id column.';
END IF;
END
$BODY$;

ALTER FUNCTION trigger_chain_key_unique_check() OWNER TO postgres;
COMMENT ON FUNCTION trigger_chain_key_unique_check() IS 'Checks that there is one and only one sequence with the given id.';

-- 18.05.2020
-- Add push notification subscribers table.

CREATE TABLE dbo."AspNetPushNotificationSubscribers"
(
    "Id" serial NOT NULL,
    "UserId" integer NOT NULL DEFAULT 0,
    "Endpoint" text NOT NULL,
    "P256dh" text NOT NULL,
    "Auth" text NOT NULL,
    CONSTRAINT "PK_dbo.AspNetPushNotificationSubscribers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_dbo.AspNetPushNotificationSubscribers_dbo.AspNetUsers_UserId" FOREIGN KEY ("UserId")
        REFERENCES dbo."AspNetUsers" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
);
COMMENT ON TABLE dbo."AspNetPushNotificationSubscribers" IS 'Table for storing data about devices that are subscribers to push notifications.';

-- 15.04.2021
-- Copy tasks results into task_result table.

INSERT INTO task_result (task_id, key, value) 
	SELECT id, 'data', result
	FROM task
	WHERE result IS NOT NULL;

INSERT INTO task_result (task_id, key, value) 
	SELECT id, 'similarityMatrix', additional_result_data 
	FROM task
	WHERE additional_result_data IS NOT NULL;

INSERT INTO task_result (task_id, key, value) 
	SELECT t.id, d.key, d.value :: json 
	FROM task t
	INNER JOIN json_each_text(t.result::json ) d ON true
	WHERE t.additional_result_data IS NOT NULL
	AND d.key IN ('characteristics', 'attributeValues');

-- 01.02.2022
-- Add collection country and collection date columns for genetic matters.

ALTER TABLE IF EXISTS matter ADD COLUMN collection_country text;

ALTER TABLE IF EXISTS matter ADD COLUMN collection_date date;

-- 10.02.2022
-- Add unique constraint on multisequence id and number in matters table.

ALTER TABLE IF EXISTS matter ADD CONSTRAINT uk_matter_multisequence UNIQUE (multisequence_id, multisequence_number);

-- 13.02.2022
-- Delete redundant columns result and additional_result_data from task table.

ALTER TABLE IF EXISTS task DROP COLUMN IF EXISTS result;

ALTER TABLE IF EXISTS task DROP COLUMN IF EXISTS additional_result_data;

-- 19.11.2022
-- Recreate matter-multisequence unique constraint as deferred.

ALTER TABLE IF EXISTS matter DROP CONSTRAINT IF EXISTS uk_matter_multisequence;

ALTER TABLE IF EXISTS matter ADD CONSTRAINT uk_matter_multisequence UNIQUE (multisequence_id, multisequence_number) DEFERRABLE INITIALLY DEFERRED;

-- 21.11.2022
-- Add collection coordinates column for GenBank sequences.

ALTER TABLE IF EXISTS matter ADD COLUMN collection_location text;

COMMIT;