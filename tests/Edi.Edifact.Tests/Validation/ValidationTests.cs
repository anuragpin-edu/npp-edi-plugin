using Edi.Core.Model;
using Edi.Edifact.Parsing;
using Edi.Edifact.Validation;
using Xunit;

namespace Edi.Edifact.Tests.Validation;

public class ValidationTests
{
    private readonly EdifactParser _parser = new();
    private readonly EdifactEnvelopeValidator _validator = new();

    /// <summary>
    /// Valid EDIFACT ORDERS message — should produce no validation issues.
    /// </summary>
    private const string ValidOrders =
        "UNA:+.? '" +
        "UNB+UNOC:3+SENDER001:ZZ+RECEIVER001:ZZ+260701:1200+00000000000001'" +
        "UNH+1+ORDERS:D:96A:UN'" +
        "BGM+220+PO-2026-00042+9'" +
        "DTM+137:20260701:102'" +
        "DTM+2:20260715:102'" +
        "NAD+BY+BUYERID001::91'" +
        "NAD+SE+SELLERID001::91'" +
        "LIN+1++WIDGET-A100:SA'" +
        "QTY+21:500:PCE'" +
        "LIN+2++GADGET-B200:SA'" +
        "QTY+21:250:PCE'" +
        "UNS+S'" +
        "UNT+12+1'" +
        "UNZ+1+00000000000001'";

    /// <summary>
    /// Message missing the UNT segment.
    /// </summary>
    private const string MissingUnt =
        "UNA:+.? '" +
        "UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ+260701:1200+00001'" +
        "UNH+1+ORDERS:D:96A:UN'" +
        "BGM+220+PO-2026-00042+9'" +
        "UNZ+1+00001'";

    /// <summary>
    /// Message with UNT segment count mismatch (says 99 but only 3 segments in message).
    /// </summary>
    private const string BadUntCount =
        "UNA:+.? '" +
        "UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ+260701:1200+00001'" +
        "UNH+1+ORDERS:D:96A:UN'" +
        "BGM+220+PO-2026-00042+9'" +
        "UNT+99+1'" +
        "UNZ+1+00001'";

    /// <summary>
    /// Message with UNT reference mismatch (UNT ref=999, UNH ref=1).
    /// </summary>
    private const string MismatchedUntRef =
        "UNA:+.? '" +
        "UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ+260701:1200+00001'" +
        "UNH+1+ORDERS:D:96A:UN'" +
        "BGM+220+PO-2026-00042+9'" +
        "UNT+3+999'" +
        "UNZ+1+00001'";

    /// <summary>
    /// Message with UNZ reference mismatch (UNZ ref=WRONG, UNB ref=00001).
    /// </summary>
    private const string MismatchedUnzRef =
        "UNA:+.? '" +
        "UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ+260701:1200+00001'" +
        "UNH+1+ORDERS:D:96A:UN'" +
        "BGM+220+PO-2026-00042+9'" +
        "UNT+3+1'" +
        "UNZ+1+WRONG'";

    /// <summary>
    /// Message missing the UNB segment.
    /// </summary>
    private const string MissingUnb =
        "UNA:+.? '" +
        "UNH+1+ORDERS:D:96A:UN'" +
        "BGM+220+PO-2026-00042+9'" +
        "UNT+3+1'" +
        "UNZ+1+00001'";

    /// <summary>
    /// Message missing the UNZ segment.
    /// </summary>
    private const string MissingUnz =
        "UNA:+.? '" +
        "UNB+UNOC:3+SENDER:ZZ+RECEIVER:ZZ+260701:1200+00001'" +
        "UNH+1+ORDERS:D:96A:UN'" +
        "BGM+220+PO-2026-00042+9'" +
        "UNT+3+1'";

    [Fact]
    public void ValidMessage_NoIssues()
    {
        // Arrange
        var doc = _parser.Parse(ValidOrders, dictionary: null);

        // Act
        var issues = _validator.Validate(doc);

        // Assert
        Assert.Empty(issues);
    }

    [Fact]
    public void MissingUnt_HasMissingUntIssue()
    {
        // Arrange
        var doc = _parser.Parse(MissingUnt, dictionary: null);

        // Act
        var issues = _validator.Validate(doc);

        // Assert
        Assert.NotEmpty(issues);
        Assert.Contains(issues, i => i.Code == "EDIFACT_MISSING_UNT");
    }

    [Fact]
    public void BadCount_HasCountMismatchIssue()
    {
        // Arrange
        var doc = _parser.Parse(BadUntCount, dictionary: null);

        // Act
        var issues = _validator.Validate(doc);

        // Assert
        Assert.NotEmpty(issues);
        Assert.Contains(issues, i => i.Code == "EDIFACT_UNT_COUNT_MISMATCH");
    }

    [Fact]
    public void MismatchedUntRef_HasRefMismatchIssue()
    {
        // Arrange
        var doc = _parser.Parse(MismatchedUntRef, dictionary: null);

        // Act
        var issues = _validator.Validate(doc);

        // Assert
        Assert.NotEmpty(issues);
        Assert.Contains(issues, i => i.Code == "EDIFACT_UNT_REF_MISMATCH");
    }

    [Fact]
    public void MismatchedUnzRef_HasRefMismatchIssue()
    {
        // Arrange
        var doc = _parser.Parse(MismatchedUnzRef, dictionary: null);

        // Act
        var issues = _validator.Validate(doc);

        // Assert
        Assert.NotEmpty(issues);
        Assert.Contains(issues, i => i.Code == "EDIFACT_UNZ_REF_MISMATCH");
    }

    [Fact]
    public void MissingUnb_HasMissingUnbIssue()
    {
        // Arrange
        var doc = _parser.Parse(MissingUnb, dictionary: null);

        // Act
        var issues = _validator.Validate(doc);

        // Assert
        Assert.NotEmpty(issues);
        Assert.Contains(issues, i => i.Code == "EDIFACT_MISSING_UNB");
    }

    [Fact]
    public void MissingUnz_HasMissingUnzIssue()
    {
        // Arrange
        var doc = _parser.Parse(MissingUnz, dictionary: null);

        // Act
        var issues = _validator.Validate(doc);

        // Assert
        Assert.NotEmpty(issues);
        Assert.Contains(issues, i => i.Code == "EDIFACT_MISSING_UNZ");
    }
}
