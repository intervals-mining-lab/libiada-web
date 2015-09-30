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


COMMIT;