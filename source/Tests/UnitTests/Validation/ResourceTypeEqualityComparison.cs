using FluentAssertions;
using IdentityServer3.Core.Validation;
using Xunit;

namespace IdentityServer3.Tests.Validation
{
    /// <summary>
    /// Tests for ResourceTypeEqualityComparer
    /// </summary>
    /// <remarks>
    /// Some of these are pretty fundamental equality checks, but the purpose here is to ensure the
    /// important property: that the order is insignificant when multiple values are
    /// sent in a space-delimited string.  We want to ensure that property holds and at the same time
    /// the basic equality function works as well.
    /// </remarks>
    public class ResourceTypeEqualityComparison
    {
        /// <summary>
        /// These tests ensure that single-value strings compare with the
        /// same behavior as default string comparisons.
        /// </summary>
        public class SingleValueStringComparisons
        {
            [Fact]
            public void Both_null()
            {
                ResourceTypeEqualityComparer comparer = new ResourceTypeEqualityComparer();
                string x = null;
                string y = null;
                var result = comparer.Equals(x, y);
                var expected = (x == y);
                result.Should().Be(expected);
            }

            [Fact]
            public void Left_null_other_not()
            {
                ResourceTypeEqualityComparer comparer = new ResourceTypeEqualityComparer();
                string x = null;
                string y = "";
                var result = comparer.Equals(x, y);
                var expected = (x == y);
                result.Should().Be(expected);
            }

            [Fact]
            public void Right_null_other_not()
            {
                ResourceTypeEqualityComparer comparer = new ResourceTypeEqualityComparer();
                string x = "";
                string y = null;
                var result = comparer.Equals(x, y);
                var expected = (x == y);
                result.Should().Be(expected);
            }

            [Fact]
            public void token_token()
            {
                ResourceTypeEqualityComparer comparer = new ResourceTypeEqualityComparer();
                string x = "token";
                string y = "token";
                var result = comparer.Equals(x, y);
                var expected = (x == y);
                result.Should().Be(expected);
            }

            [Fact]
            public void id_token_id_token()
            {
                ResourceTypeEqualityComparer comparer = new ResourceTypeEqualityComparer();
                string x = "id_token";
                string y = "id_token";
                var result = comparer.Equals(x, y);
                var expected = (x == y);
                result.Should().Be(expected);
            }

            [Fact]
            public void id_token_token()
            {
                ResourceTypeEqualityComparer comparer = new ResourceTypeEqualityComparer();
                string x = "id_token";
                string y = "token";
                var result = comparer.Equals(x, y);
                var expected = (x == y);
                result.Should().Be(expected);
            }
        }

        /// <summary>
        /// These tests ensure the property demanded by the 
        /// <see href="https://tools.ietf.org/html/rfc6749#section-3.1.1">OAuth2 spec</see>
        /// where, in a space-delimited list of values, the order is not important.
        /// </summary>
        public class MultipleValueStringComparisons
        {
            [Fact]
            public void id_token_token_both_ways()
            {
                ResourceTypeEqualityComparer comparer = new ResourceTypeEqualityComparer();
                string x = "id_token token";
                string y = "token id_token";
                var result = comparer.Equals(x, y);
                result.Should().BeTrue();
            }

            [Fact]
            public void code_id_token_both_ways()
            {
                ResourceTypeEqualityComparer comparer = new ResourceTypeEqualityComparer();
                string x = "code id_token";
                string y = "id_token code";
                var result = comparer.Equals(x, y);
                result.Should().BeTrue();
            }

            [Fact]
            public void code_token_both_ways()
            {
                ResourceTypeEqualityComparer comparer = new ResourceTypeEqualityComparer();
                string x = "code token";
                string y = "token code";
                var result = comparer.Equals(x, y);
                result.Should().BeTrue();
            }

            [Fact]
            public void code_id_token_token_combo1()
            {
                ResourceTypeEqualityComparer comparer = new ResourceTypeEqualityComparer();
                string x = "code id_token token";
                string y = "id_token code token";
                var result = comparer.Equals(x, y);
                result.Should().BeTrue();
            }

            [Fact]
            public void code_id_token_token_combo2()
            {
                ResourceTypeEqualityComparer comparer = new ResourceTypeEqualityComparer();
                string x = "code id_token token";
                string y = "token id_token code";
                var result = comparer.Equals(x, y);
                result.Should().BeTrue();
            }

            [Fact]
            public void code_id_token_token_missing_code()
            {
                ResourceTypeEqualityComparer comparer = new ResourceTypeEqualityComparer();
                string x = "code id_token token";
                string y = "id_token token";
                var result = comparer.Equals(x, y);
                result.Should().BeFalse();
            }

            [Fact]
            public void code_id_token_token_missing_code_and_token()
            {
                ResourceTypeEqualityComparer comparer = new ResourceTypeEqualityComparer();
                string x = "code id_token token";
                string y = "id_token";
                var result = comparer.Equals(x, y);
                result.Should().BeFalse();
            }

            [Fact]
            public void Totally_different_words()
            {
                ResourceTypeEqualityComparer comparer = new ResourceTypeEqualityComparer();
                string x = "blerg smoo";
                string y = "token code";
                var result = comparer.Equals(x, y);
                result.Should().BeFalse();
            }

            [Fact]
            public void Same_length_different_count()
            {
                ResourceTypeEqualityComparer comparer = new ResourceTypeEqualityComparer();
                string x = "code id_token token";
                string y = "tokenizer bleegerfi";
                var result = comparer.Equals(x, y);
                result.Should().BeFalse();
            }
        }
    }
}
