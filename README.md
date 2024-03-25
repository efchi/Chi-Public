
# Chi Language

v0.1.5 | By [@efchi](https://github.com/efchi)

The Chi Language (pronounced [/tʃiː/](http://ipa-reader.xyz/?text=%20%2Ft%CA%83i%CB%90%2F)) is a toy scripting language written in C# and .NET Core.

The language was born during my sabbatical as an hobby project, and will likely remain such. However, I'll try to expand it in the future to incorporate more features, with the objective of making it similar to a real programming language.

The project includes a fully handwritten lexer and parser, as well as a basic interpreter. It could be useful for learning purposes, as it shows how to build a simple language from scratch with no dependencies - how to code a trivial lexer, how to implement a LL(k) recursive descent parser to create an AST, and so on. 

Chi was originally conceived as a macro-language for performing substitutions on tree structures, but it has since evolved into something more complex and with a semantics that resembles functional programming and symbolic programming. 

Chi has a C-like syntax and is influenced by many languages such as Lisp, Python, C#, JavaScript, and others.

## Main Features

Here's an overview of the Chi language.

### Expressions and Values

Chi programs are expressions, and expressions produce values when evaluated. Chi currently uses the prefix notation for arithmetical operators, that basically means calling a primitive function to perform such operations.

	+(42, 1)	# 42 + 1 ==> 43

#### Arithmetic Operators

	+(x, y)		# x + y	| Sum
	-(x, y)		# x - y | Subtraction
	-(x)		# -x	| Negation
	++(x)		# x + 1 | Increment
	--(x)		# x - 1 | Decrement
	mul(x, y)	# x * y | Multiplication
	div(x, y)	# x / y | Division
	mod(x, y)	# x % y | Modulus
	
Chi values can be atomic or structured. For now, the only supported atomic value type is *Number*, used for working with both integer and floating point numbers and implemented on the C# *decimal* type.

### Function Definitions

A function definition is a parameterized name-expression binding.

	def A(x) => x;		# A(42) ==> 42

If a function doesn't have any parameter, the parameter list can be omitted.

	def B => A(42);		# B ==> 42

### Open Identifiers

The signature feature of Chi is the presence of *open* identifiers or symbols. An open identifier is an identifier that is not yet defined.

When an identifier is evaluated, if it has been previously defined, then it is replaced with the expression bound to its definition. Otherwise, it is kept as a symbol. This is similar to the concept of *free variable* in mathematics.

	def A(x) => x;		# A(42) ==> 42 (Number)
	def B => x;		# B() ==> x (Open)

The code above defines two functions named A and B. **A** takes a parameter **x** and immediately returns it, while **B** takes no parameters and returns the symbol **x**, that may be captured or *closed* later during execution (see the *close-with* construct).

Similarly to other languages, where entities like objects, functions or types are said to be first-class citizens, we could say that in Chi identifiers are first-class citizens, since they can be freely passed around the program and evaluated at need.

This feature has of course a lot of drawbacks, but still I believe it has a lot of potential because it could enable different ways to tackle problems with a flexible, dynamic and expressive approach.

### Auto-Normalizing Sequences

In Chi, every piece of code is an expression, which ultimately produces a value. But every value is also a *sequence* of one element. This is similar to Lisp, where almost everything is a list. 

A sequence is a list of one or more juxtaposed elements:

	This is a sequence of open identifiers

Sequences can also be explicitly constructed using the concatenation operator:

	This :: is :: also :: a :: sequence :: of :: open :: identifiers

Which can be used as a syntax disambiguator when needed.

	A(X)		# Apply function A to expression X
	A :: (X)	# Concatenate A and expression X

The **empty sequence** is represented by the *Nil* value, that is the identity element for the concatenation operator.

	nil			# Nil
	1 nil 2 nil 3 nil	# 1 2 3

**Sequence normalization** is the process of flattening nested sequences and removing nil values. This is done to ensure that every sequence is in its normal form, that is, it does not contain other sequences as elements or nil values inside.

The following examples shows how nested sequences resulting from a function application are normalized into one:

	def A(x) => x x;
	A(1) :: A(2)		# 1 1 2 2

Nested sequences are always flattened when evaluated:

	((1 1) (2 2))		# 1 1 2 2
	1 2 (3 (4 5)) 6		# 1 2 3 4 5 6
	nil ((nil) :: nil)	# Nil
	
This semantics implies that sequences cannot be used to build nested structures, but they can be used to build ordered sets of items. In Chi, trees are built using tuples instead, which preserve structure information.

### Tuples

*Tuples* are used to represent nested structures. A tuple is a list of one or more elements enclosed in curly braces **and** separated by the comma character. 

	{ 1, 2, 3 }		# A tuple of 3 elements
	{ 1, { 2, 3 } }		# A tuple of 2 elements. The second is a nested tuple

Unlike sequences, tuples are never flattened, and nil values inside a tuple are preserved. For that reason, you should be careful to not mix the structure and tuple syntax, since they are not semantically the same.

	{ 1, 2, 3 }		# A tuple of 3 elements, each one is a Number
	{ 1 2 3 }		# A tuple of 1 element, that is a sequence of 3 Numbers: 1 2 3

	{ 1, 2, nil }		# A tuple of 3 elements, the third is Nil
	{ 1 2 nil }		# A tuple of 1 element, that is a Sequence of 2 Numbers: 1 2

Tuples can be used to build trees or arrays. I'm planning to extend them in the future, adding support for named fields. 

### Close-With Construct

The *close-with* construct is used in combination with open identifiers, and it's the main feature of the language. Close-with is used to capture open identifiers during the evalation of an expression.

This basically means to enforce a **dynamic scoping** rule to resolve a subset of open identifiers.

	def T => Mary had a little lamb;
	close T() with (Mary = Johnny);			# Johnny had a little lamb

Another example: 

	def T(x) => Name is a person of age x;		# x is a function parameter
	T(42)						# ==> Name is a person of age 42
	close T(42) with (Name = John);			# ==> John is a person of age 42

This feature can be used to create object instances, like in the example above, or even to create (instances of) generic types without having to specify or require explicit type parameters. Other possible applications are configuration, dependency injection, method name injection, etc.

## Other Features

### Comments

As seen previously, comments are specified with the # character. Only single-line comments are currently supported.

	# This is a comment

### Scoping

Chi currently supports lexical scoping for global and local variables; for now, there is no support for nested scopes, non-local variables, closures and so on. As explained above, the close-with construct can be used to create a dynamic scope when needed, by capturing open identifiers.

### Wildcard Operator

The wildcard operator is currently used to refer to the last expression result.

	+(42, 1)	# 43
	_		# 43

### Conditional Expressions

Conditional expressions (*if-then*, *if-then-else*) are used to evaluate different expressions based on a condition. In Chi, the Number 0 is considered *false*, while any other Number is considered *true*. There is currently no boolean type.

	if 1 then 42 else 99		# 42
	if 0 then 42 else 99		# 0

#### Comparison Operators

	eq(x, y)	# == | Equal
	neq(x, y)	# != | Not equal
	gt(x, y)	# >  | Greater than
	lt(x, y)	# <  | Less than
	gte(x, y)	# >= | Greater than or equal
	lte(x, y)	# <= | Less than or equal

#### Logical Operators

	and(x, y)	# x and y
	or(x, y)	# x or y
	not(x, y)	# not x
	xor(x, y) 	# x xor y

### New Operator and State Objects

The *new* operator is used to create a mutable object. Chi objects are called *States*. They are used to store and manipulate data, and are basically dynamic dictionaries of key-value pairs. See the next section for examples of how to use states through assigment and field lookup.

	new()			# A new State object

### Variables and Assignment

Variables are mutable name-value bindings, defined using the *var* keyword. You can define a variable in local scope (inside a *def*) or global scope. The *set* keyword is used to assign a value to a variable.

	var (x = 42);		# x ==> 42
	set (x = 99);		# x ==> 99

**State variables**. If no value is assigned during a variable definition, the variable is initialized with a new state:

	var (x);		# Shorthand for var (x = new())
	
**Nested assignments**. Assignments can be nested using the *access operator* and *index operator*.

	var y = field;		# y ==> field (Open)
	set (x.y = 42);		# x.y ==> 42
	set (x.[y] = 99);	# x.field ==> 99

In the example above, **x** must be a previously defined variable (local or global) or a function parameter of type *State*.

When using the access operator (the second line) **y** is interpreted as an open identifier. When using the index operator instead (the third line) **y** can be any expression, which will be evaluated and its result (which is, again, an open identifier) is used as an index. This resembles JavaScript objects, which can be accessed like dictionaries using strings as keys, with the difference that here we don't use strings but syntactical symbols.

### Language Testing

#### Test Construct

The *test* construct is used to perform a test case and returns a boolean value. We use it to enforce the language sematics and correctness across the project evolution and implementation.

You can see some examples in ./sources/.test/ directory.

	test factorial-naive
	{
		def F(x) => 
			if eq(x, 0) then (1) else mul(x, F(--(x)));

		check(F(10), 3628800);
	};

#### Structural Equivalence

In the example above, the *check* function is used to assess the structural equivalence of two values. Structural equivalence is a primitive accessible with the *steq* name, and is recursively defined this way:

- Two *atoms* (eg. numbers) are structurally equivalent if they are equal
- Two *sequences* are structurally equivalent if they have the same length and all their elements are structurally equivalent, *taking into account the sequence normalization rule*
- Two *tuples* are structurally equivalent if they have the same length and all their elements are structurally equivalent
- Two *states* are structurally equivalent if they have the same keys and all their values are structurally equivalent.

In the example above, the *check* ensures that the computed result of the factorial function F(10) is equal to its correct value (3628800).

### Math Library

Using only primitives and the language features it was possible to build a library of simple mathematical functions. You can find it in .sources/.startup/math.chi.

Here is an example:

	def power(a, b) =>
		if is-zero(b) then 1
		else mul(a, power(a, --(b)));

Please note that many functions were suggested by GitHub Copilot and some of them may not be completely correct!

## Running Chi

### REPL Mode

Execute the program with no arguments to run in REPL mode. You can run the command **/usage** to see the list of available commands. Here is an example of output:

	/quit                    Quit the Chi REPL.
	/usage                   Shows the Chi REPL usage.
	/option <key> <value>    Set Option. <key>: [verbose|whitespace|comments], <value>: [true|false].
	/source <source>         Use <source> for subsequent commands.
	/file <filename>         Load <filename> for subsequent commands.
	/lex                     Run Lexer on current source, producing some tokens.
	/post                    Run Postprocessor on current tokens, producing some tokens.
	/lex-post                Run Lexer and Postprocessor on current source, producing some tokens.
	/parse                   Run Parser on current tokens, producing an AST
	/lex-post-parse          Run Lexer, Postprocessor and Parser on current source, producing an AST.
	/run                     Run Interpreter on current AST, producing a result value (accessible with _).
	/lex-post-parse-run      Run Interpreter on current source, producing a result value (accessible with _)
	/reset                   Reset the REPL and Interpreter state.
	/test                    Run all language tests.

### File Mode

Execute the program with a file name as an argument to run in file mode.

	chi .sources/.sample.chi

## Other Aspects

### Autoloaded Files

Files under the folder .sources/.startup/ (eg. the math.chi file) are automatically loaded every time the interpreter is started (or restarted).

### Output and Buffering

Chi includes the *print* primitive function to write values to the console. When using print (or any other console write operation) the output is buffered on a background thread. This is a useful approach to improve the main thread performance and responsiveness.

### Sequence Optimization

The interpreter contains a boolean flag to enable or disable sequence optimization. This is an attempt to improve the interpreter performance, potentially speeding-up sequence normalization in some cases.

### Chi Grammar

You can find a gramamar specification for the language inside the file .docs/grammar.txt.

## Upcoming Features

Chi is still in early development. I would like to extend it and implement more features in the future. Some examples are: 

- Support for iteration and more complex control flow instructions
- Improving current data structures (eg. support for named tuples)
- Implementation of a real type system
- Support for real closures and anonymous functions
- Support for LINQ-like operators (eg. select, where, etc)
- Modules (using / include directives)
- Performance benchmarks.

... and many more!

## License and Contacts

Chi is released under the MIT License (LICENSE.md file). You are free to contribute to the project, fork it, or use it for your own purposes. If you'd like to contact me, please reach out to (info [at] federicocorrao [dot] it).
