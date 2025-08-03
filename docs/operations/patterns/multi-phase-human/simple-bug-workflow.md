# Simple Bug Investigation Workflow

> **Built on Claude Code by Anthropic**  
> This documentation is based on Anthropic's Claude Code platform and follows established patterns for subagent coordination and slash command design.

## Horizontal Flow Pattern - Leveraging Native Claude Coordination

```mermaid
flowchart LR
    A[/development/bugs/investigate-bug<br/>payment issue] --> B[@planners/bug-investigator] --> C[bug-report.md] --> D{Fix now?} 
    D -->|yes| E[/development/bugs/fix-bug<br/>payment issue] --> F[@executors/bug-fixer] --> G[fix-complete.md]
    D -->|no| H[Manual review]
    
    %% Commands (Yellow)
    style A fill:#fff2cc,stroke:#d6b656,stroke-width:2px,color:#000
    style E fill:#fff2cc,stroke:#d6b656,stroke-width:2px,color:#000
    
    %% Planners (Green)
    style B fill:#d5e8d4,stroke:#82b366,stroke-width:2px,rx:10,ry:10,color:#000
    
    %% Executors (Orange)
    style F fill:#ffd6cc,stroke:#d79b00,stroke-width:2px,rx:10,ry:10,color:#000
    
    %% Documents (White)
    style C fill:#ffffff,stroke:#000000,stroke-width:2px,color:#000
    style G fill:#ffffff,stroke:#000000,stroke-width:2px,color:#000
    
    %% Human decisions (Red diamond)
    style D fill:#f8cecc,stroke:#b85450,stroke-width:2px,color:#000
    
    %% Manual actions (Purple)
    style H fill:#e1d5e7,stroke:#9673a6,stroke-width:2px,color:#000
```

## Native Claude Coordination

**Command Logic (Simplified):**
```markdown
---
description: Investigate bug with systematic analysis
argument-hint: [bug description]
---

Use @planners/bug-investigator to systematically investigate: $ARGUMENTS

Require 95% confidence before concluding investigation.
```

**Claude handles:**
- Agent selection based on descriptions
- Context management between agents  
- Sequential chaining when requested
- Quality assessment through agent prompts

**Human handles:**
- Decision points between phases
- Quality validation
- Process oversight

## Human Decision Point

**Gate: Fix or Escalate**
- **Input**: Complete bug analysis from @planners/bug-investigator
- **Decision**: Is this a simple fix or complex issue requiring escalation?
- **Options**:
  - ✅ Fix now: Issue is straightforward, proceed with @executors/bug-fixer
  - 🔄 Manual review: Issue is complex, escalate to senior team
  - 🔍 More investigation: Need deeper analysis before decision

**Decision Criteria:**
- Investigation confidence level > 95%
- Fix complexity assessment (simple vs complex)
- Risk level of proposed solution
- Time sensitivity of the issue

## Command Examples

```bash
# Simple bug investigation with fix option
/development/bugs/investigate-bug "Login button not responding on mobile"

# Claude coordinates:
# 1. Uses @planners/bug-investigator to analyze the mobile login issue
# 2. Presents findings with confidence level and fix recommendation
# 3. Human decides: simple fix or escalate for complex analysis
# 4. If fix approved, @executors/bug-fixer implements solution immediately
```

## Pattern Effectiveness

**When to Use Simple Sequential:**
- Straightforward bugs with likely simple fixes
- Time-sensitive issues requiring quick resolution
- Low-risk scenarios where fast iteration is preferred
- Well-understood problem domains

**Success Metrics:**
- 90% of simple bugs resolved in single workflow
- 95% investigation accuracy for fix/escalate decisions
- 3x faster resolution for appropriate bug types
- Zero escalated issues that should have been simple fixes

**Why This Works:**
- **Optimized for speed**: Minimal overhead for simple cases
- **Clear decision criteria**: Human knows exactly what to evaluate
- **Efficient escalation**: Complex issues quickly routed to experts
- **Context preservation**: Fix phase has full investigation context
- **Risk appropriate**: Matches process complexity to issue complexity

---

## References and Attribution

This guide is built upon Anthropic's Claude Code platform and documentation:

- [Claude Code Subagents](https://docs.anthropic.com/en/docs/claude-code/sub-agents)
- [Claude Code Slash Commands](https://docs.anthropic.com/en/docs/claude-code/slash-commands)
- [Claude Code Overview](https://docs.anthropic.com/en/docs/claude-code/overview)

Claude Code is developed by [Anthropic](https://www.anthropic.com/).