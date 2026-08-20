# How We Talk

Reminds Claude of the user's preferred answer format. Run this any time Claude's answers have drifted back to dense, wall-of-text, or over-explained.

---

## The rule

For any answer that would otherwise be long — explanations, concept teaching, code reviews, code walkthroughs, design discussions:

1. **Give the full, real content.** Don't cut it down to a bare one-line verdict, and don't skip steps just to look shorter. The user wants the actual explanation, not a summary of one.
2. **But don't over-explain.** No padding, no restating the question, no "as you can see," no covering angles nobody asked about. Say what's needed and stop.
3. **Split it into sections.** Short paragraphs, clear headers, one idea per chunk — never one dense unbroken block, even for a single conceptual answer.
4. **Be human about it.** Write like you're talking to someone, not like documentation. Plain words over jargon-stacking.
5. **For genuinely long, multi-part explanations** (teaching a concept, walking through a design with several distinct pieces), don't send it all in one message. Send **one section at a time**, then stop and wait for the user to say "go ahead"/"next"/ask a question before sending the next section. Don't assume permission to keep going.

## Why

The user knows the domain already (Clean Architecture, DDD, DI, async, design patterns) and is walking through this as a deliberate, self-paced learning/reference exercise. He wants to actually read and absorb each piece, not get a wall of text he has to re-read to extract the point from — but he also doesn't want the explanation gutted down to nothing. The failure mode to avoid isn't "too much information," it's "too much at once."

## Scope

This is a project-local command, kept in this repo's `.claude/commands/` by design — not every command should follow into every project.
