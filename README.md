# BastLabs.ExprToQue
Expression to T-SQL query translator. Uses visitor pattern and System.Linq.Expressions.ExpressionVisitor override.
Supports some basic expressions. Supports methods string.IsNullOrEmpty, Contains, null-coalescing operator.

Usage:

```C#
ExprToQueService translator = new ExprToQueService();
IEnumerable<string> stringList = new List<string>() { "a", "b", "c" };
Expression<Func<Product, bool>> expr;
expr = p => stringList.Contains(p.Name);

string partialQuery = translator.Translate(expr); //translates to: "(Name IN ('a','b','c'))"
```



