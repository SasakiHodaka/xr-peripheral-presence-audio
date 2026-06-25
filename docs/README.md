# Documentation Index

This directory separates the research explanation from the Unity implementation
notes. Read the documents in this order when preparing for a research meeting,
presentation, or thesis chapter.

## 1. Research Story

- `RESEARCH_OVERVIEW.md`
  - Background, problem, purpose, novelty, hypotheses, scope, and expected contribution.
- `RELATED_WORK_QA.md`
  - Short answers for IVAS, MASA, object-based audio, semantic communication,
    audio tokens, turn taking, and likely defense questions.
- `SCENE_TOKEN_SPEC.md`
  - Formal Scene Token definition, fields, token generation, and token-based rendering.

## 2. Prototype and Experiment

- `ARCHITECTURE.md`
  - Unity component responsibilities and data flow.
- `EXPERIMENT_PROTOCOL.md`
  - How to run the mock experiment, collect logs, and check data quality.
- `PROJECT_STATUS.md`
  - Current prototype state, validation status, and known limitations.
- `NEXT_STEPS.md`
  - Implementation, experiment, and writing tasks in priority order.

## Current Research Claim

```text
Scene Token integrates spatial information and conversation-state information
into a discrete representation for VR spatial voice communication. Compared
with spatial metadata alone, it can support not only sound reproduction but also
conversation understanding.
```

## What Not to Overclaim Yet

- Do not claim full automatic semantic communication yet.
- Do not claim proven bandwidth reduction yet.
- Do not claim IVAS or MASA codec implementation.
- Do not claim real multi-user networking yet.

The current prototype is a controlled Unity mock scene for evaluating the
representation and rendering effect of Scene Tokens.
