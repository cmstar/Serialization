# cmstar.Serialization

A light weight JSON serialization tool.

## JsonSerializer
The JsonSerializer class is the entry for serializing/deserializing.

### Serialize CLR objects to JSONs

    class Data
    {
        public string String { get; set; }
        public int Int { get; set; }
        public int[] Array { get; set; }
    }

    var serializer = new JsonSerializer();

    serializer.Serialize(123);
    //-> 123

    serializer.Serialize("Hello\nWorld");
    //-> "Hellow\nWorld"

    serializer.Serialize(DateTime.Now.ToUniversalTime());
    //-> "\/Date(1377498441115)\/"

    serializer.Serialize(new char[] { 'a', 'b' });
    //-> ["a","b"]

    serializer.Serialize(new Dictionary<string, int> {
        { "key1", 1 },
        { "key2", 2 }
    });
    //-> {"key1":1,"key2":2}

    serializer.Serialize(new Data { Array = new int[] { 1, 2 } });
    //-> {"String":null,"Int":0,"Array":[1,2]}

### Deserialize JSONs to CLR objects

    //non-generic version
    Data data = (Data)serializer.Deserialize(
        "{\"String\":null,\"Int\":0,\"Array\":[1,2]}",
        typeof(Data));

    //generic version
    int[] array = serializer.Deserialize<int[]>("[1,2,3]");

### The default JsonSerializer

Each instance of JsonSerializer is isolated, it can keep different instances of JsonContract and can be customized separately from another JsonSerializer. But in most time, we need just one JsonSerializer, in the case we can use JsonSerializer.Default:

    var json = JsonSerializer.Default.Serialize(new Data());
    var data = JsonSerializer.Default.Deserialize<Data>(json);


### Using Attributes

To serialize a POCO, by default, only public properties will be serialized.
You can use the JsonPropertyAttribute to customize the serializing:

    class Data
    {
        public Data(string s) { String = s; }

        [JsonProperty("string_value")] //mark a private field
        private string String;

        [JsonProperty] //no name specified, will use the original name
        public int Int { get; set; }

        public int WillBeIngored { get; set; }
    }

    JsonSerializer.Default.Serialize(new Data("s") { Int = 3 });
    //-> {"string_value":"s","Int":3}

or use JsonIgnoreAttribute:

    class Data
    {
        public string String { get; set; }

        [JsonJsonIgnore]
        public int Int { get; set; }
    }

    JsonSerializer.Default.Serialize(new Data { String = "s", Int = 3 });
    //-> {"String":"s"}

### Pretty-print JSON

By default the JSONs outputted is compact but not much human-readable. An overload of the JsonSerializer.Serialize() mothod accepts an argument 'formatting' which can be used to specify the format of JSON seriliazed.

    JsonSerializer.Default.Serialize(new Data { Array = new int[] { 1, 2 } });
    //-> {"String":null,"Int":0,"Array":[1,2]}

    JsonSerializer.Default.Serialize(
        new Data { Array = new int[] { 1, 2 } },
        Formatting.Multiple);
    /* ->
    {
    "String":null,
    "Int":0,
    "Array":[
    1,
    2
    ]
    }
    */

    JsonSerializer.Default.Serialize(
        new Data { Array = new int[] { 1, 2 } },
        Formatting.Indented);
    /* ->
    {
        "String":null,
        "Int":0,
        "Array":[
            1,
            2
        ]
    }
    */

### Faster Serialization

For performance need, the JsonSerializer.FastSerialize() method provides a faster serialization, which is about 50% faster than the JsonSerializer.Serialize() method.

The faster version uses the JsonWriter class against the JsonWriterImproved class used by the JsonSerializer.Serialize() method. See the chapter below for more details.

## JsonContract

The classes derive from the JsonContract class indicate how to serialize CLR objects or deserialize JSONs.

The table below gives out the default contracts, which will convert the CLR types to/from corresponding JSON types:

|CLR type|Contract|JSON type|
|----|----|----|
|String|StringContract|String|
|Char|StringContract|String|
|Boolean|BooleanContract|Boolean|
|SByte|NumberContarct|Integer|
|Int16|NumberContarct|Integer|
|Int32|NumberContarct|Integer|
|Int64|NumberContarct|Integer|
|Byte|NumberContarct|Integer|
|UInt16|NumberContarct|Integer|
|UInt32|NumberContarct|Integer|
|UInt64|NumberContarct|Integer|
|IntPtr|NumberContarct|Integer|
|UIntPtr|NumberContarct|Integer|
|Single|NumberContarct|Float|
|Double|NumberContarct|Float|
|Decimal|NumberContarct|Float|
|DateTime|DateTimeContarct|String|
|Guid|GuidContarct|String|
|Enum|EnumContarct|Integer|
|IDictionary|DictionaryContarct|Object|
|IDictionary<,>|DictionaryContarct|Object|
|ICollection|ArrayContarct|Array|
|ICollection<>|ArrayContarct|Array|
|Object|ObjectContarct|Object|

### Serializing DateTime

### Serializing Enum

## JsonWriter

## JsonReader