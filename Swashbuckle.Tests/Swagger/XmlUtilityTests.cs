using NUnit.Framework;
using Swashbuckle.Swagger.XmlComments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Swashbuckle.Tests.Swagger
{
    /// <summary>
    /// Tests for XmlUtility
    /// 
    /// NB: Whitespace in these tests is significant and uses a combination of {tabs} and {spaces}
    /// 
    /// You should toggle "View White Space" to "on".
    /// 
    /// Visual Studio 
    ///     shortcut: CTRL + R, CTRL + W)
    ///     menu    : Edit > Advanced > View White Space
    /// </summary>
    [TestFixture]
    public class XmlUtilityTests
    {
        [Test]
        public void XmlComment_returns_verbatim_from_single_line_input()
        {
            string input = @"My single line comment";
            string expected = @"My single line comment";
            string actual = XmlUtility.NormalizeIndentation(input);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void XmlComment_returns_verbatim_from_padding_single_line_input()
        {
            string input = @"
    My single line indented comment
";
            string expected = @"My single line indented comment";
            string actual = XmlUtility.NormalizeIndentation(input);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void XmlComment_trims_common_leading_whitespace_over_all_lines()
        {
            string input = @"
            ## Test Heading
            
            Another line of text
            
              * list item 1
              * list item 2
            
            Third paragraph";

            string expected = @"## Test Heading

Another line of text

  * list item 1
  * list item 2

Third paragraph";

            string actual = XmlUtility.NormalizeIndentation(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void XmlComment_does_not_apply_trimming_if_no_common_sequence_found()
        {
            string input = @"
            ## Test Heading
            
I'm a line affecting the leading whitespace
            
              * list item 1
              * list item 2
            
            Third paragraph";

            string expected = @"            ## Test Heading
            
I'm a line affecting the leading whitespace
            
              * list item 1
              * list item 2
            
            Third paragraph";

            string actual = XmlUtility.NormalizeIndentation(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void XmlComment_honours_code_blocks_when_finding_common_leading_whitespace()
        {
            string input = @"
            ## Test Heading
            
            Another line of text
            
            	var object = {
            		""key1"": value,
            		""key2"": value
            	}
            ";

            string expected = 
@"## Test Heading

Another line of text

	var object = {
		""key1"": value,
		""key2"": value
	}";

            string actual = XmlUtility.NormalizeIndentation(input);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// common indentation seen in visual studio: {tab}{space}
        /// </summary>
        [Test]
        public void XmlComment_handles_mixed_indendation_using_tab_space()
        {
            string input = @"
	 ## Test Heading
	 
	 Another line of text
	 
	 	var object = {
	 		""key1"": value,
	 		""key2"": value
	 	}
";

            string expected = 
@"## Test Heading

Another line of text

	var object = {
		""key1"": value,
		""key2"": value
	}";

            string actual = XmlUtility.NormalizeIndentation(input);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// common indentation seen in visual studio: {space}{tab}
        /// </summary>
        [Test]
        public void XmlComment_handles_mixed_indendation_using_space_tab()
        {
            string input = @"
 	## Test Heading
 	
 	Another line of text
 	
 		var object = {
 			""key1"": value,
 			""key2"": value
 		}
";

            string expected = 
@"## Test Heading

Another line of text

	var object = {
		""key1"": value,
		""key2"": value
	}";

            string actual = XmlUtility.NormalizeIndentation(input);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// This particular test ensures leading whitespace is consistent over _all_ lines
        /// </summary>
        [Test]
        public void XmlComment_detects_consistent_space_indendation()
        {
            string input = @"
    Space Indentation Line 1
    Space Indentation Line 2
	Misplaced Tab Indentation
    Space Indentation Line 4
";

            string expected = 
@"    Space Indentation Line 1
    Space Indentation Line 2
	Misplaced Tab Indentation
    Space Indentation Line 4";

            string actual = XmlUtility.NormalizeIndentation(input);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// This particular test ensures leading whitespace is consistent over _all_ lines
        /// </summary>
        [Test]
        public void XmlComment_detects_consistent_tab_indendation()
        {
            string input = @"
	Tab Indentation Line 1
	Tab Indentation Line 2
    Misplaced Space Indentation
	Tab Indentation Line 4
";

            string expected =
@"	Tab Indentation Line 1
	Tab Indentation Line 2
    Misplaced Space Indentation
	Tab Indentation Line 4";

            string actual = XmlUtility.NormalizeIndentation(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void XmlComment_handles_mixed_complex_content()
        {
            string input = @"
            Some details about this
            method and why you'd like to use it.
            
            Here's an example of posting a new `TestModel` to the test endpoint.
            
                POST /api/test
                
                {
                  ""prop1"": {
                    ""name"": ""value"",
                    ...
                  },
                  ""prop2"": {
                    ""name"": ""value"",
                    ...
                  }
                }
";

            string expected = @"Some details about this
method and why you'd like to use it.

Here's an example of posting a new `TestModel` to the test endpoint.

    POST /api/test
    
    {
      ""prop1"": {
        ""name"": ""value"",
        ...
      },
      ""prop2"": {
        ""name"": ""value"",
        ...
      }
    }";

            string actual = XmlUtility.NormalizeIndentation(input);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// NB: Having a code block as the first line means it can't discern the indentation correctly
        /// </summary>
        [Test]
        public void XmlComment_handles_code_on_the_first_line_poorly()
        {
            string input = @"
    POST /api/test
    
    {
      ""prop1"": {
        ""name"": ""value"",
        ...
      },
      ""prop2"": {
        ""name"": ""value"",
        ...
      }
    }
";

            string expectedButUndesired = 
@"POST /api/test

{
  ""prop1"": {
    ""name"": ""value"",
    ...
  },
  ""prop2"": {
    ""name"": ""value"",
    ...
  }
}";

            string actual = XmlUtility.NormalizeIndentation(input);

            Assert.AreEqual(expectedButUndesired, actual);
        }

         /// <summary>
        /// NB: Having a code block as the first line means it can't discern the indentation correctly
        /// </summary>
        [Test]
        public void XmlComment_handles_code_on_the_first_line_when_subsequent_non_code_lines_are_present()
        {
            string input = @"
    POST /api/test
    
    {
      ""prop1"": {
        ""name"": ""value"",
        ...
      },
      ""prop2"": {
        ""name"": ""value"",
        ...
      }
    }

The above is a sample code block
";

            string expectedButUndesired = 
@"    POST /api/test
    
    {
      ""prop1"": {
        ""name"": ""value"",
        ...
      },
      ""prop2"": {
        ""name"": ""value"",
        ...
      }
    }

The above is a sample code block";

            string actual = XmlUtility.NormalizeIndentation(input);

            Assert.AreEqual(expectedButUndesired, actual);
        }
    }
}
