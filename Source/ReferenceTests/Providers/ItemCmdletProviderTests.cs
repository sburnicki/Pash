﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using TestPSSnapIn;

namespace ReferenceTests.Providers
{   
    [TestFixture]
    public class ItemCmdletProviderTests : ReferenceTestBaseWithTestModule
    {
        private const string _providerQualification = TestItemProvider.ProviderName + "::";
        private const string _fooItemValue = "bar";
        private const string _setFooItemCommand = "Set-Item -Path '" + _providerQualification + "foo' -Value '"
            + _fooItemValue + "'";

        [TestCase("foo:bar", true)]
        [TestCase("foo12", false)] // TestItemProvider only allows a-z, \, and :
        public void ItemProviderCanValidatePath(string path, bool valid)
        {
            var cmd = "Test-Path -IsValid '" + _providerQualification + ":" + path + "'";
            ExecuteAndCompareTypedResult(cmd, valid);
        }

        [Test]
        public void ItemProviderCanValidateMutliplePaths()
        {
            var paths = String.Join("','", from p in new [] { "foo:bar", "ab:?:c", "baz:foo\\" }
                                                select _providerQualification + p);
            var cmd = "Test-Path -IsValid '" + paths + "'";
            ExecuteAndCompareTypedResult(cmd, true, false, true);
        }

        [Test]
        public void ItemProviderCanGetItem()
        {
            var cmd = "Get-Item -Path '" + _providerQualification + TestItemProvider.DefaultItemName + "'";
            ExecuteAndCompareTypedResult(cmd, TestItemProvider.DefaultItemValue);
        }

        [Test]
        public void ItemProviderWithoutFilterCapabilitiesFailsOnParameter()
        {
            var cmd = "Get-Item -Filter 'foo' -Path '" + _providerQualification + TestItemProvider.DefaultItemName + "'";
            Assert.Throws<CmdletProviderInvocationException>(delegate {
                ExecuteAndCompareTypedResult(cmd, TestItemProvider.DefaultItemValue);
            });
        }

        [Test]
        public void ItemProviderCanClearItem()
        {
            var cmd = NewlineJoin(
                "Clear-Item -Path '" + _providerQualification + TestItemProvider.DefaultItemName + "'",
                "Get-Item -Path '" + _providerQualification + TestItemProvider.DefaultItemName + "'"
            );
            ExecuteAndCompareTypedResult(cmd, new object[0]); // no output
        }

        [Test]
        public void ItemProviderCanSetItem()
        {
            var cmd = _setFooItemCommand + " -PassThru";
            ExecuteAndCompareTypedResult(cmd, _fooItemValue);
        }

        [Test]
        public void ItemProviderCanInvokeItem()
        {
            var cmd = "Invoke-Item -Path '" + _providerQualification + TestItemProvider.DefaultItemName + "'";
            ExecuteAndCompareTypedResult(cmd, "invoked!"); // generated by the InvokeItem function
        }

        [Test]
        public void ItemProviderCanGetMultipleItems()
        {
            var cmd = NewlineJoin(
                _setFooItemCommand,
                "Get-Item -Path '" + _providerQualification + "*'"
            );
            ExecuteAndCompareTypedResult(cmd, TestItemProvider.DefaultItemValue, _fooItemValue);
        }

        [Test]
        public void ItemProviderCanSetMultipleItems()
        {
            var cmd = NewlineJoin(
                _setFooItemCommand,
                "Get-Item -Path '" + _providerQualification + "*'", // assure we have the two desired items
                "Set-Item -Path " + _providerQualification + "* -Value 'baz'", // set them
                "Get-Item -Path '" + _providerQualification + "*'" // show the result
            );
            ExecuteAndCompareTypedResult(cmd, TestItemProvider.DefaultItemValue, _fooItemValue, "baz", "baz");
        }

        [Test]
        public void ItemProviderCanClearMultipleItems()
        {
            var cmd = NewlineJoin(
                _setFooItemCommand,
                "Get-Item -Path '" + _providerQualification + "*'", // assure we have the two desired items
                "Clear-Item -Path '" + _providerQualification + "*'", // clear them
                "Get-Item -Path '" + _providerQualification +"*'" // shouldn't output anything
            );
            ExecuteAndCompareTypedResult(cmd, TestItemProvider.DefaultItemValue, _fooItemValue); // no output after clear
        }

        [Test]
        public void ItemProviderCanInvokeMultipleItems()
        {
            var cmd = NewlineJoin(
                _setFooItemCommand,
                "Get-Item -Path '" + _providerQualification + "*'", // assure we have the two desired items
                "Invoke-Item -Path '" + _providerQualification + "*'" // invoke them
            );
            ExecuteAndCompareTypedResult(cmd, TestItemProvider.DefaultItemValue, _fooItemValue, "invoked!", "invoked!");
        }

    }
}
