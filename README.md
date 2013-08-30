# cmstar.Serialization

A light weight JSON serialization library written in C#.

## JsonSerializer
The `JsonSerializer` class is the entry for serializing/deserializing.

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

### Anonymous Objects

Serializing anonymouse objects is just the same:

    var anonymousObject = new {
        Foo = 123,
        Bar = "xx",
        Array = new int[] { 1, 2, 3 }
    };
    JsonSerializer.Default.Serialize(anonymousObject);
    //-> {"Foo":123,"Bar":"xx","Array":[1,2,3]}

To deserialize, a template object should be provided:

    var template = new { Foo = 0, Bar = (string)null };
    var json = "{\"Foo\":10,\"Bar\":\"s\"}";

    //call JsonSerializer.Deserialize<T>(string json, T template)
    var result = JsonSerializer.Default.Deserialize(json, template);

### The default JsonSerializer

Each instance of `JsonSerializer` is isolated, it can keep different instances of `JsonContract` and can be customized separately from another `JsonSerializer`. But in most time, we need just one `JsonSerializer`, in the case we can use `JsonSerializer.Default`:

    var json = JsonSerializer.Default.Serialize(new Data());
    var data = JsonSerializer.Default.Deserialize<Data>(json);


### Using Attributes

To serialize a POCO, by default, only public properties will be serialized.
You can use the `JsonPropertyAttribute` to select the members you need:

    class Data
    {
        public Data(string s) { String = s; }

        [JsonProperty("string_value")] //mark a private field
        private string String;

        [JsonProperty] //no name specified, will use 'Int' directly
        public int Int { get; set; }

        public int WillBeIngored { get; set; }
    }

    JsonSerializer.Default.Serialize(new Data("s") { Int = 3 });
    //-> {"string_value":"s","Int":3}

or use `JsonIgnoreAttribute`:

    class Data
    {
        public string String { get; set; }

        [JsonJsonIgnore]
        public int Int { get; set; }
    }

    JsonSerializer.Default.Serialize(new Data { String = "s", Int = 3 });
    //-> {"String":"s"}

Note: If you mix `JsonIgnoreAttribute` and `JsonPropertyAttribute` together, the  serializer ignores `JsonPropertyAttribute`.

Note: If a property has no getter accessor (public or non-public), it will be ignored during the serialization; and the value of a property without a setter accessor will not be set.

### Pretty-print JSON

By default the JSONs outputted is compact but not much human-readable. An overload of the `JsonSerializer.Serialize()` mothod accepts an argument 'formatting' which can be used to specify the format of JSON seriliazed.

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

For performance need, the `JsonSerializer.FastSerialize()` method provides a faster serialization, which is about 50% faster than the `JsonSerializer.Serialize()` method.

The faster version uses the `JsonWriter` class against the `JsonWriterImproved` class used by the `JsonSerializer.Serialize()` method. See the 'JsonWriter' section below for more details.

## JsonContract

The classes derive from the `JsonContract` class indicate how to serialize CLR objects or deserialize JSONs.

The table below gives out the default contracts, which will convert the CLR types to/from corresponding JSON types:

|CLR type|Contract|JSON type|
|----|----|----|
|String|StringContract|String|
|Char|StringContract|String|
|Boolean|BooleanContract|Boolean|
|SByte|NumberContarct|Number|
|Int16|NumberContarct|Number|
|Int32|NumberContarct|Number|
|Int64|NumberContarct|Number|
|Byte|NumberContarct|Number|
|UInt16|NumberContarct|Number|
|UInt32|NumberContarct|Number|
|UInt64|NumberContarct|Number|
|IntPtr|NumberContarct|Number|
|UIntPtr|NumberContarct|Number|
|Single|NumberContarct|Number|
|Double|NumberContarct|Number|
|Decimal|NumberContarct|Number|
|DateTime|DateTimeContarct|String|
|Guid|GuidContarct|String|
|Nullable&lt;T&gt;|NullableTypeContract|Depends on typeof(T)|
|Types derived from Enum|EnumContarct|Number|
|Implementations of IDictionary|DictionaryContarct|Object|
|Implementations of IDictionary&lt;K,V&gt;|DictionaryContarct|Object|
|Implementations of ICollection|ArrayContarct|Array|
|Implementations of ICollection&lt;T&gt;|ArrayContarct|Array|
|Other types not listed above|ObjectContarct|Object|

- CLR `null` (`Nothing` in VB.net) will be serialized to JSON `null`.
- If an object is `Nullable<T>` and has value, it will be serialized using the  underlying value; otherwise, will be serialized to JSON `null`.
- Non-generic implementations of `ICollection` or `IDictionary` can be serialized but can not be deserialized because when deserializing the application doesn't know which CLR type should be used - the JSON type to CLR type mapping is 1 to N. 

### JsonContractResolver

This class is used to resolve the `JsonContract`s for different types.

You can register custom `JsonContract`s by sending a dictionary to the constructor of `JsonContractResolver`:

    var customContracts = new Dictionary<Type, JsonContract>();
    customContracts.Add(typeof(Data), new CustomDataContract());

    var contractResolver = new JsonContractResolver(customContracts);
    var serializer = new JsonSerializer(contractResolver);

Note: You can't register custom `JsonContract`s to `JsonSerializer.Default` at present. 

### Serializing Dates

The default contract for `DateTime` is the `DateTimeContract`, which will serialize dates in the Microsoft format such as "\/Date(1377498441115+0600)\/". Another contract provided is the `CustomFormatDateTimeContract` with a property `Format`, the code below shows how to serialize dates in the format "yyyy~MM~dd HH:mm:ss":

    var dateTimeContract = new CustomFormatDateTimeContract();
    dateTimeContract.Format = "yyyy~MM~dd HH@mm@ss";

    var customContracts = new Dictionary<Type, JsonContract>();
    customContracts.Add(typeof(DateTime), dateTimeContract);

    var contractResolver = new JsonContractResolver(customContracts);
    var serializer = new JsonSerializer(contractResolver);

    serializer.Serialize(DateTime.Now);
    //-> "2013~07~15 14@41@03"

### Serializing Enums

By default enums are serialized to JSON numbers using the index, if you need the name of an enum, you can setup the `EnumContract.UseEnumName` property to `true`.

    var stringEnumContract = new EnumContract(typeof(SomEnum));
    stringEnumContract.UseEnumName = true;

    var customContracts = new Dictionary<Type, JsonContract>();
    customContracts.Add(typeof(SomEnum), stringEnumContract);

    var contractResolver = new JsonContractResolver(customContracts);
    var serializer = new JsonSerializer(contractResolver);

    serializer.Serialize(SomEnum.SomeItem);
    //-> "SomeItem"


### Customize the resolving of JsonContracts

Here is an example that shows how to tell the `JsonSerializer` to serialize all enums by their names.

First, build a sub class of the `JsonContractResolver` and override the `DoResove` method which is the core method for contract resolving:

    class StringEnumContractResolver : JsonContractResolver
    {
        protected override JsonContract DoResolve(Type type)
        {
            if (type.IsSubclassOf(typeof(Enum)))
                return new EnumContract(type) { UseEnumName = true };

            return base.DoResolve(type);
        }
    }

Then you can setup the `JsonSerializer` with the class above:

    var contractResolver = new StringEnumContractResolver();
    var serializer = new JsonSerializer(contractResolver);

    serializer.Serialize(SomeEnum.SomeItem);
    //-> "SomeItem"

## JsonWriter

## JsonReader