# drycodegen

通过为`Visual Studio`提供扩展来生成一些`C++`代码，目前的实现：

> 在类声明位置嵌入`__`，则触发自动完成，键盘/鼠标选择，即可自动生成序列化、反序列化代码。

![](assets/ar_autocomplete.png)

![](assets/ar_autocomplete_result.png)

后续将会补充各种序列化库代码支持，或者其它由于`C++`语言没有反射导致的重复代码问题。

