using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using EDBTypes;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public static class NpgsqlFullTextSearchLinqExtensions
    {
        /// <summary>
        /// AND tsquerys together. Generates the "&amp;&amp;" operator.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static EDBTsQuery And([NotNull] this EDBTsQuery query1, [NotNull] EDBTsQuery query2)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsQuery) + "." + nameof(And)));

        /// <summary>
        /// OR tsquerys together. Generates the "||" operator.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static EDBTsQuery Or([NotNull] this EDBTsQuery query1, [NotNull] EDBTsQuery query2)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsQuery) + "." + nameof(Or)));

        /// <summary>
        /// Negate a tsquery. Generates the "!!" operator.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static EDBTsQuery ToNegative([NotNull] this EDBTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsQuery) + "." + nameof(ToNegative)));

        /// <summary>
        /// Returns whether <paramref name="query1" /> contains <paramref name="query2" />.
        /// Generates the "@&gt;" operator.
        /// http://www.postgresql.org/docs/current/static/functions-textsearch.html
        /// </summary>
        public static bool Contains([NotNull] this EDBTsQuery query1, [NotNull] EDBTsQuery query2)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsQuery) + "." + nameof(Contains)));

        /// <summary>
        /// Returns whether <paramref name="query1" /> is contained within <paramref name="query2" />.
        /// Generates the "&lt;@" operator.
        /// http://www.postgresql.org/docs/current/static/functions-textsearch.html
        /// </summary>
        public static bool IsContainedIn([NotNull] this EDBTsQuery query1, [NotNull] EDBTsQuery query2)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsQuery) + "." + nameof(IsContainedIn)));

        /// <summary>
        /// Returns the number of lexemes plus operators in <paramref name="query" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static int GetNodeCount([NotNull] this EDBTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsQuery) + "." + nameof(GetNodeCount)));

        /// <summary>
        /// Get the indexable part of <paramref name="query" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static string GetQueryTree([NotNull] this EDBTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsQuery) + "." + nameof(GetQueryTree)));

        /// <summary>
        /// Returns a string suitable for display containing a query match.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-HEADLINE
        /// </summary>
        public static string GetResultHeadline([NotNull] this EDBTsQuery query, [NotNull] string document)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsQuery) + "." + nameof(GetResultHeadline)));

        /// <summary>
        /// Returns a string suitable for display containing a query match.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-HEADLINE
        /// </summary>
        public static string GetResultHeadline(
            [NotNull] this EDBTsQuery query,
            [NotNull] string document,
            [NotNull] string options)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsQuery) + "." + nameof(GetResultHeadline)));

        /// <summary>
        /// Returns a string suitable for display containing a query match using the text
        /// search configuration specified by <paramref name="config" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-HEADLINE
        /// </summary>
        public static string GetResultHeadline(
            [NotNull] this EDBTsQuery query,
            [NotNull] string config,
            [NotNull] string document,
            [NotNull] string options)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsQuery) + "." + nameof(GetResultHeadline)));

        /// <summary>
        /// Searches <paramref name="query" /> for occurrences of <paramref name="target" />, and replaces
        /// each occurrence with a <paramref name="substitute" />. All parameters are of type tsquery.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static EDBTsQuery Rewrite(
            [NotNull] this EDBTsQuery query,
            [NotNull] EDBTsQuery target,
            [NotNull] EDBTsQuery substitute)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsQuery) + "." + nameof(Rewrite)));

        /// <summary>
        /// Returns a tsquery that searches for a match to <paramref name="query1" /> followed by a match
        /// to <paramref name="query2" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static EDBTsQuery ToPhrase(
            [NotNull] this EDBTsQuery query1,
            [NotNull] EDBTsQuery query2)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsQuery) + "." + nameof(ToPhrase)));

        /// <summary>
        /// Returns a tsquery that searches for a match to <paramref name="query1" /> followed by a match
        /// to <paramref name="query2" /> at a distance of <paramref name="distance" /> lexemes using
        /// the &lt;N&gt; tsquery operator
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY
        /// </summary>
        public static EDBTsQuery ToPhrase(
            [NotNull] this EDBTsQuery query1,
            [NotNull] EDBTsQuery query2,
            int distance)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsQuery) + "." + nameof(ToPhrase)));

        /// <summary>
        /// This method generates the "@@" match operator. The <paramref name="query"/> parameter is
        /// assumed to be a plain search query and will be converted to a tsquery using plainto_tsquery.
        /// http://www.postgresql.org/docs/current/static/textsearch-intro.html#TEXTSEARCH-MATCHING
        /// </summary>
        public static bool Matches([NotNull] this EDBTsVector vector, [NotNull] string query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(Matches)));

        /// <summary>
        /// This method generates the "@@" match operator.
        /// http://www.postgresql.org/docs/current/static/textsearch-intro.html#TEXTSEARCH-MATCHING
        /// </summary>
        public static bool Matches([NotNull] this EDBTsVector vector, [NotNull] EDBTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(Matches)));

        /// <summary>
        /// Returns a vector which combines the lexemes and positional information of <paramref name="vector1" />
        /// and <paramref name="vector2"/> using the || tsvector operator. Positions and weight labels are retained
        /// during the concatenation.
        /// https://www.postgresql.org/docs/10/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static EDBTsVector Concat([NotNull] this EDBTsVector vector1, [NotNull] EDBTsVector vector2)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(Concat)));

        /// <summary>
        /// Assign weight to each element of <paramref name="vector" /> and return a new
        /// weighted tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static EDBTsVector SetWeight(
            [NotNull] this EDBTsVector vector,
            EDBTsVector.Lexeme.Weight weight)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(SetWeight)));

        /// <summary>
        /// Assign weight to elements of <paramref name="vector" /> that are in <paramref name="lexemes" /> and
        /// return a new weighted tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static EDBTsVector SetWeight(
            [NotNull] this EDBTsVector vector,
            EDBTsVector.Lexeme.Weight weight,
            [NotNull] string[] lexemes)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(SetWeight)));

        /// <summary>
        /// Assign weight to each element of <paramref name="vector" /> and return a new
        /// weighted tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static EDBTsVector SetWeight([NotNull] this EDBTsVector vector, char weight)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(SetWeight)));

        /// <summary>
        /// Assign weight to elements of <paramref name="vector" /> that are in <paramref name="lexemes" /> and
        /// return a new weighted tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static EDBTsVector SetWeight(
            [NotNull] this EDBTsVector vector,
            char weight,
            [NotNull] string[] lexemes)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(SetWeight)));

        /// <summary>
        /// Return a new vector with <paramref name="lexeme" /> removed from <paramref name="vector" />
        /// https://www.postgresql.org/docs/current/static/functions-textsearch.html
        /// </summary>
        public static EDBTsVector Delete([NotNull] this EDBTsVector vector, [NotNull]  string lexeme)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(Delete)));

        /// <summary>
        /// Return a new vector with <paramref name="lexemes" /> removed from <paramref name="vector" />
        /// https://www.postgresql.org/docs/current/static/functions-textsearch.html
        /// </summary>
        public static EDBTsVector Delete([NotNull] this EDBTsVector vector, [NotNull] string[] lexemes)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(Delete)));

        /// <summary>
        /// Returns a new vector with only lexemes having weights specified in <paramref name="weights" />.
        /// https://www.postgresql.org/docs/current/static/functions-textsearch.html
        /// </summary>
        public static EDBTsVector Filter([NotNull] this EDBTsVector vector, [NotNull] char[] weights)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(Filter)));

        /// <summary>
        /// Returns the number of lexemes in <paramref name="vector" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static int GetLength([NotNull] this EDBTsVector vector)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(GetLength)));

        /// <summary>
        /// Removes weights and positions from <paramref name="vector" /> and returns
        /// a new stripped tsvector.
        /// http://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR
        /// </summary>
        public static EDBTsVector ToStripped([NotNull] this EDBTsVector vector)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(ToStripped)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float Rank([NotNull] this EDBTsVector vector, [NotNull] EDBTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(Rank)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> while normalizing
        /// the result according to the behaviors specified by <paramref name="normalization" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float Rank(
            [NotNull] this EDBTsVector vector,
            [NotNull] EDBTsQuery query,
            NpgsqlTsRankingNormalization normalization)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(Rank)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> with custom
        /// weighting for word instances depending on their labels (D, C, B or A).
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float Rank(
            [NotNull] this EDBTsVector vector,
            [NotNull] float[] weights,
            [NotNull] EDBTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(Rank)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> while normalizing
        /// the result according to the behaviors specified by <paramref name="normalization" />
        /// and using custom weighting for word instances depending on their labels (D, C, B or A).
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float Rank(
            [NotNull] this EDBTsVector vector,
            [NotNull] float[] weights,
            [NotNull] EDBTsQuery query,
            NpgsqlTsRankingNormalization normalization)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(Rank)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover
        /// density method.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float RankCoverDensity(
            [NotNull] this EDBTsVector vector,
            [NotNull] EDBTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(RankCoverDensity)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover
        /// density method while normalizing the result according to the behaviors specified by
        /// <paramref name="normalization" />.
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float RankCoverDensity(
            [NotNull] this EDBTsVector vector,
            [NotNull] EDBTsQuery query,
            NpgsqlTsRankingNormalization normalization)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(RankCoverDensity)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover
        /// density method with custom weighting for word instances depending on their labels (D, C, B or A).
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float RankCoverDensity(
            [NotNull] this EDBTsVector vector,
            [NotNull] float[] weights,
            [NotNull] EDBTsQuery query)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(RankCoverDensity)));

        /// <summary>
        /// Calculates the rank of <paramref name="vector" /> for <paramref name="query" /> using the cover density
        /// method while normalizing the result according to the behaviors specified by <paramref name="normalization" />
        /// and using custom weighting for word instances depending on their labels (D, C, B or A).
        /// http://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-RANKING
        /// </summary>
        public static float RankCoverDensity(
            [NotNull] this EDBTsVector vector,
            [NotNull] float[] weights,
            [NotNull] EDBTsQuery query,
            NpgsqlTsRankingNormalization normalization)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(EDBTsVector) + "." + nameof(RankCoverDensity)));
    }
}
