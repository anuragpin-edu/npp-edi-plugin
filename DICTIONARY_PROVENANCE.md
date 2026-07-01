# Dictionary Provenance

This document records the origin and licensing status of EDI segment/element dictionaries used in NppEdiPlugin.

---

## Phase 1 — EDIFACT Dictionary

### Source

The Phase 1 EDIFACT dictionary is **hand-authored** from publicly available UN/EDIFACT documentation:

- **UN/EDIFACT Standard**: [ISO 9735](https://www.iso.org/standard/17592.html) — Electronic data interchange for administration, commerce and transport (EDIFACT) — Application level syntax rules.
- **UN/EDIFACT Directories**: Published by [UNECE](https://unece.org/trade/uncefact/unedifact/directories) under public access.

### Covered Segments

| Segment | Description |
|---|---|
| UNB | Interchange Header |
| UNH | Message Header |
| BGM | Beginning of Message |
| DTM | Date/Time/Period |
| NAD | Name and Address |
| LIN | Line Item |
| QTY | Quantity |
| UNS | Section Control |
| UNT | Message Trailer |
| UNZ | Interchange Trailer |

### What Is NOT Used

- **Bots-EDI grammar files** — Licensed under GPL. These files have **not** been embedded, copied, or used as a reference for this dictionary. The Bots project (https://github.com/bots-edi/bots) uses the GNU General Public License v3, which is incompatible with the MIT license of this project unless explicit relicensing is obtained.

### Verification

All segment tags, element names, and qualifier descriptions were manually looked up from:

1. The UNECE Trade Facilitation & E-Business (UN/CEFACT) public directory listings.
2. ISO 9735 syntax rule descriptions (service segments UNB, UNH, UNT, UNZ).
3. Publicly available EDIFACT message type documentation (e.g., ORDERS, INVOIC).

No automated extraction or copy-paste from GPL-licensed codebases was performed.

---

## Future Phases

| Phase | Standard | Dictionary Source |
|---|---|---|
| Phase 2 | X12 | To be determined — likely hand-authored from ASC X12 public documentation |
| Phase 3 | VDA | To be determined — from VDA public specs |

Dictionary provenance will be documented here before each phase ships.
