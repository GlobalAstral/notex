---@diagnostic disable: lowercase-global

---@param str string;
function outputPush(str) end

---@param str string?;
function outputPushln(str) end

---@param package string;
---@param properties string?;
function addPackage(package, properties) end

---@param directive string;
function addAfterPackage(directive) end

---@param footer string;
function addFooter(footer) end

---@param language string;
function addLanguage(language) end

---@alias DocumentConfigs {
  --- class: string;
  --- fontSize: integer;
  --- paper: string;
  --- orientation: string;
  --- side: string;
  --- column: string;
  --- draft: boolean;
  --- title: string?;
  --- author: string?;
  --- date: string;
  --- paragraphIndent: integer;
  --- paragraphSkip: integer;
  --- lineSpread: number;
  --- sectionNumberDepth: integer;
  --- tableOfContents: boolean;
  --- tableOfContentsDepth: integer;
  --- pageNumbering: string;
  --- pageNumber: integer;
  --- pageStyle: string;
  --- geometry: string;
---}

---@param configs DocumentConfigs;
function setDocument(configs) end

---@param label string;
---@param content string;
function addFootnote(label, content) end

---@param name string;
---@param properties table;
function addCustomContainer(name, properties) end

---@param func fun();
---@param priority integer?;
function registerSetup(func, priority) end

---@param func fun();
---@param priority integer?;
function registerWithPackages(func, priority) end

---@param func fun();
---@param priority integer?;
function registerAfterPackages(func, priority) end

---@param func fun();
---@param priority integer?;
function registerAfterRun(func, priority) end

---@param value any;
function print(value) end

---@param value any;
function println(value) end

---@param name string;
---@param default table;
function getOrCreateConfig(name, default) end
