#!/bin/bash
# Note: we need to modify EdifactParser to parse version from UNH and X12Parser to parse version from ISA or GS.
# For now, let's just use string? version = null; in the parse methods if we can't extract it easily, 
# or just change EdiDocument to accept version and we pass null.
# But we *want* to extract the version!

# Let's inspect EdifactParser first.
cat src/Edi.Edifact/Parsing/EdifactParser.cs
