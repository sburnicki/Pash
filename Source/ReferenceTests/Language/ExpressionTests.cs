using System;
using NUnit.Framework;

namespace ReferenceTests.Language
{
    [TestFixture]
    public class ExpressionTests : ReferenceTestBase
    {

        // a void value is not written to pipeline (see GeneralConversionTest.ConvertToVoidNotWrittenToPipeline)
        // but it can be used in an expression as null. Here are just some examples
        [TestCase("[string]::IsNullOrEmpty([void]0)")]
        [TestCase("[void]'foo' -eq $null")]
        [TestCase("$a = [void]'foo'; $var = Get-Variable a; ($var.name -eq 'a') -and ($var.value -eq $null)")]
        public void EmptyPipeExpressionIsHandledAsNull(string cmd)
        {
            ExecuteAndCompareTypedResult(cmd, true);
        }

        [TestCase("$null", new object[0])]
        [TestCase("$null, 'b', $null", new [] {null, "b", null})]
        [TestCase("[void]'a'", new object[0])]
        [TestCase("[void]'a', 'b'", new [] {null, "b"})]
        public void SingleNullValueInParenthesisIsIgnored(string parenExpression, object[] expected) {
            ExecuteAndCompareTypedResult("(" + parenExpression + ")", expected);
        }


    }
}

