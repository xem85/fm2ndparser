# JSON Specification

Complete reference for the JSON produced by `Fm2ndParser`. All examples below are
**illustrative** (hand-built from the data model) — values are placeholders.

## Conventions

- **Property names** are `camelCase`. Acronyms are lowercased as a run, so `.NET`
  names map like `BGM → bgm`, `WL → wl`, `RI → ri`, `CDDATrack → cddaTrack`,
  `HRatio → hRatio`, `P1CursorPosX → p1CursorPosX`.
- **Enums serialize as integers**, not strings. Every enum-typed field is documented
  as `int` with a link to the [Enums](#enums) appendix for the value→name mapping.
  (Exception: several `type` fields are plain strings the parser sets explicitly —
  those are noted individually.)
- **Byte arrays** (`data`, `pointer`, palettes) serialize as **base64 strings**.
  `globalPalettes` is therefore an array of base64 strings.
- **Derived (read-only) fields** are computed from other fields and are still emitted.
  They are read-only outputs; setting them has no meaning. Each is marked *(derived)*.
- **Integer widths** (`byte`, `short`, `ushort`, `int`, `uint`) are given to indicate
  range and signedness; in JSON they are all plain numbers.
- Reference-typed fields (e.g. a `SkillReference`) are `null` when absent.

### Output "modes"

Two output shapes exist depending on the `--clean-up` (`-c`) flag. Every field table
has a **Modes** column:

| Marker         | Meaning |
|----------------|---------|
| `both`         | Present and identical in default and `--clean-up` output. |
| `normal-only`  | Present in **default** output; **removed** by `--clean-up`. |

`--clean-up` is additionally **lossy**: beyond removing `normal-only` fields it mutates
some values (merges consecutive `I` blocks, zeroes certain `I`/`FA` fields, resets all
block `index` to 0). See [cli-usage.md](cli-usage.md#what---clean-up-does-lossy). Fields
affected by that mutation are flagged in the relevant block tables.

The complete set of `normal-only` fields is: `skill.index`, `imageResource.data`,
`imageResource.offset`, `soundResource.data`, and `number` on `SkillReference` /
`SkillBlockReference`.

Two properties are **never** emitted (ignored in both modes): a block's raw `data`
bytes and a skill's internal block `position`.

---

## Top-level envelope (all file types)

Every output file — `game`, `character`, `stage`, `demo` — shares this base shape,
then adds type-specific fields (see [Per-file-type fields](#per-file-type-fields)).

| Field            | Type                    | Modes | Notes |
|------------------|-------------------------|-------|-------|
| `type`           | string                  | both  | `"game"`, `"character"`, `"stage"`, or `"demo"`. |
| `name`           | string                  | both  | File / game name. |
| `skills`         | [Skill](#skill)[]       | both  | All skills (animations/actions). |
| `images`         | [ImageResource](#imageresource)[] | both | Embedded images. |
| `globalPalettes` | string[]                | both  | Base64 palettes (typically 8, indices 0–7), 1024 bytes each. Used for palette swaps — see [output-formats.md](output-formats.md). |
| `sounds`         | [SoundResource](#soundresource)[] | both | Embedded sounds. |
| `bgm`            | [SkillReference](#skillreference) | both | Background-music reference (nullable). |
| `time`           | uint                    | both  | File-level timing value. |

```jsonc
{
  "type": "character",
  "name": "Ryu",
  "skills": [ /* Skill objects */ ],
  "images": [ /* ImageResource objects */ ],
  "globalPalettes": ["<base64>", "<base64>", "..."],
  "sounds": [ /* SoundResource objects */ ],
  "bgm": { "name": "Theme", "number": 3 },
  "time": 0,
  // ...character-specific fields follow
}
```

---

## Shared objects

### ImageResource

| Field         | Type   | Modes        | Notes |
|---------------|--------|--------------|-------|
| `width`       | uint   | both         | Pixels. |
| `height`      | uint   | both         | Pixels. |
| `paletteType` | uint   | both         | `1` = embedded palette in `data`; otherwise uses `globalPalettes`. See [output-formats.md](output-formats.md#two-palette-sources). |
| `packedSize`  | uint   | both         | Compressed size in the source. |
| `offset`      | uint   | normal-only  | Offset within the source file. |
| `data`        | string | normal-only  | Base64. Pixel indices, prefixed by a 1024-byte palette when `paletteType == 1`. |
| `pointer`     | string | both         | Base64 pointer/metadata bytes. |

### SoundResource

| Field         | Type   | Modes        | Notes |
|---------------|--------|--------------|-------|
| `name`        | string | both         | |
| `size`        | uint   | both         | |
| `data`        | string | normal-only  | Base64 raw audio bytes. |
| `endlessLoop` | bool   | both         | |
| `cddaTrack`   | byte   | both         | CD-DA track number. |
| `type`        | int    | both         | [SoundType](#soundtype). |
| `pointer`     | string | both         | Base64 pointer/metadata bytes. |

### Rgba

Standalone color object (used by `ColorBlock`, `EBBlock`, `AIBlock`).

| Field | Type | Modes | Notes |
|-------|------|-------|-------|
| `r`   | byte | both  | |
| `g`   | byte | both  | |
| `b`   | byte | both  | |
| `a`   | byte | both  | Alpha. |

### SkillReference

A pointer to another skill by name and/or index.

| Field    | Type   | Modes       | Notes |
|----------|--------|-------------|-------|
| `name`   | string | both        | Resolved skill name (nullable). |
| `number` | ushort | normal-only | Skill index. |

### SkillBlockReference

Extends [SkillReference](#skillreference) (so it also has `name` + `number`) with a
target block inside the referenced skill.

| Field       | Type   | Modes       | Notes |
|-------------|--------|-------------|-------|
| `name`      | string | both        | (inherited) |
| `number`    | ushort | normal-only | (inherited) |
| `block`     | byte   | both        | Index of the target block within the skill. |
| `blockType` | string | both        | `type` string of the target block. |

### CommonImage

Entry of a character's `commonImages` array.

| Field    | Type   | Modes | Notes |
|----------|--------|-------|-------|
| `number` | ushort | both  | |
| `x`      | short  | both  | |
| `y`      | short  | both  | |

---

## Skill

An animation/action, composed of ordered blocks.

| Field      | Type                  | Modes       | Notes |
|------------|-----------------------|-------------|-------|
| `type`     | uint                  | both        | Numeric skill category. |
| `index`    | int                   | normal-only | Position of the skill in the file. |
| `name`     | string                | both        | |
| `settings` | [SettingsBlock](#settingsblock--settings) \| null | both | **Derived**: equals `blocks[0]` when the first block is a Settings block, else `null`. The same object also appears as `blocks[0]`. |
| `blocks`   | [Block](#blocks)[]    | both        | Ordered blocks. |

```jsonc
{
  "type": 0,
  "index": 12,
  "name": "Standing",
  "settings": { "index": 0, "type": "Settings", /* ... */ },
  "blocks": [
    { "index": 0, "type": "Settings", /* ... */ },
    { "index": 1, "type": "I", "wait": 10, "i": 3, "x": 0, "y": 0, /* ... */ }
  ]
}
```

---

## Blocks

Every block object has these base fields plus the block-specific fields below:

| Field   | Type   | Modes | Notes |
|---------|--------|-------|-------|
| `index` | int    | both  | Position within the skill's `blocks`. Reset to `0` by `--clean-up`. |
| `type`  | string | both  | Discriminator string (see each block). **Note:** this is a fixed string set by the parser, *not* an enum index, and it does not always match the internal class name. |

The `type` string identifies the block. The full set:

| `type`     | Section | | `type`   | Section |
|------------|---------|-|----------|---------|
| `Settings` | [↓](#settingsblock--settings) | | `GS`   | [↓](#gsblock--gs) |
| `I`        | [↓](#iblock--i) | | `GL`     | [↓](#glblock--gl) |
| `FA`       | [↓](#fablock--fa) | | `GP`   | [↓](#gcblock--gp) |
| `FD`       | [↓](#fdblock--fd) | | `R`    | [↓](#rblock--r) |
| `O`        | [↓](#oblock--o) | | `RC`     | [↓](#rcblock--rc) |
| `C`        | [↓](#cblock--c) | | `RP`     | [↓](#rpblock--rp) |
| `M`        | [↓](#mblock--m) | | `Rnd`    | [↓](#rndblock--rnd) |
| `V`        | [↓](#vblock--v) | | `DS`     | [↓](#dsblock--ds) |
| `S`        | [↓](#sblock--s) | | `PS`     | [↓](#psblock--ps) |
| `COM`      | [↓](#comblock--com) | | `COLOR` | [↓](#colorblock--color) |
| `SC`       | [↓](#scblock--sc) | | `EB`   | [↓](#ebblock--eb) |
| `SF`       | [↓](#sfblock--sf) | | `AI`   | [↓](#aiblock--ai) |
| `SG`       | [↓](#sgblock--sg) | | `E`    | [↓](#eblock--e) |
| | | | `Unknown` | [↓](#unknownblock--unknown) |

All block fields below are `both` unless noted.

### SettingsBlock — `"Settings"`

| Field           | Type  | Notes |
|-----------------|-------|-------|
| `level`         | uint  | |
| `settingsType`  | int   | [SettingsType](#settingstype). |
| `position`      | int   | [HitMarkPosition](#hitmarkposition). |
| `numberWidth`   | byte  | |
| `time`          | uint  | |
| `x`             | short | |
| `y`             | short | |
| `width`         | short | |
| `height`        | short | |
| `connectLtRt`   | bool  | |
| `connectUpDw`   | bool  | |
| `widthEnabled`  | bool  | |
| `heightEnabled` | bool  | |

### IBlock — `"I"`

Image/frame block.

| Field             | Type   | Notes |
|-------------------|--------|-------|
| `wait`            | ushort | Frames to wait. `--clean-up` sums this across merged consecutive `I` blocks. |
| `i`               | ushort | Image index. Zeroed by `--clean-up`. |
| `x`               | short  | Zeroed by `--clean-up`. |
| `y`               | short  | Zeroed by `--clean-up`. |
| `turnX`           | bool   | Zeroed by `--clean-up`. |
| `turnY`           | bool   | Zeroed by `--clean-up`. |
| `ignoreDirection` | bool   | |

### FABlock — `"FA"`

Attack hitbox.

| Field            | Type  | Notes |
|------------------|-------|-------|
| `x`              | short | Zeroed by `--clean-up`. |
| `y`              | short | Zeroed by `--clean-up`. |
| `width`          | short | Zeroed by `--clean-up`. |
| `height`         | short | Zeroed by `--clean-up`. |
| `number`         | byte  | |
| `cancel`         | bool  | |
| `noDetection`    | bool  | |
| `combo`          | bool  | |
| `noSkyDetection` | bool  | |
| `guardFail`      | bool  | |
| `duringGuard`    | bool  | |
| `duringReceipt`  | bool  | |
| `halfed`         | bool  | |
| `power`          | byte  | |

### FDBlock — `"FD"`

Defense/vulnerable box.

| Field               | Type  | Notes |
|---------------------|-------|-------|
| `x`                 | short | |
| `y`                 | short | |
| `width`             | short | |
| `height`            | short | |
| `number`            | byte  | |
| `collide`           | bool  | |
| `damaged`           | bool  | |
| `throw`             | bool  | |
| `damageRate`        | byte  | |
| `damageRateEnabled` | bool  | *(derived)* equals `damaged`. |

### OBlock — `"O"`

Object/sub-image reference.

| Field         | Type  | Notes |
|---------------|-------|-------|
| `in`          | bool  | *(derived)* `!out && !point`. |
| `out`         | bool  | |
| `point`       | bool  | |
| `unCond`      | bool  | |
| `shadow`      | bool  | |
| `parent`      | bool  | |
| `picXY`       | bool  | |
| `skill`       | [SkillBlockReference](#skillblockreference) | |
| `outSkill`    | [SkillBlockReference](#skillblockreference) | |
| `x`           | short | |
| `y`           | short | |
| `number`      | byte  | |
| `depth`       | byte  | |
| `depthEnabled`| bool  | *(derived)* `!point`. |

### CBlock — `"C"`

Cancel/condition block.

| Field                   | Type  | Notes |
|-------------------------|-------|-------|
| `sound`                 | short | |
| `fails`                 | bool  | *(derived)* `!hits && !uncond`. |
| `hits`                  | bool  | |
| `uncond`                | bool  | |
| `levelCancelCondition`  | bool  | *(derived)* `!skillCancelCondition`. |
| `skillCancelCondition`  | bool  | |
| `from`                  | byte  | |
| `to`                    | byte  | |
| `skill`                 | [SkillReference](#skillreference) | |

### MBlock — `"M"`

Movement/gravity.

| Field           | Type  | Notes |
|-----------------|-------|-------|
| `gravityX`      | short | |
| `moveX`         | short | |
| `moveY`         | short | |
| `gravityY`      | short | |
| `add`           | bool  | |
| `replace`       | bool  | *(derived)* `!add`. |
| `stopMoveX`     | bool  | |
| `stopMoveY`     | bool  | |
| `stopGravityX`  | bool  | |
| `stopGravityY`  | bool  | |

### VBlock — `"V"`

Variable operation.

| Field            | Type   | Notes |
|------------------|--------|-------|
| `multiCondSkill` | [SkillBlockReference](#skillblockreference) | |
| `var`            | byte   | |
| `varName`        | string | |
| `replace`        | bool   | |
| `add`            | bool   | |
| `itsTheSame`     | bool   | |
| `itsAbove`       | bool   | |
| `itsBelow`       | bool   | |
| `useEven`        | bool   | |
| `useEvenVar`     | byte   | |
| `useEvenVarName` | string | |
| `value`          | short  | |
| `multiCondValue` | short  | |

### SBlock — `"S"`

Sound trigger.

| Field   | Type | Notes |
|---------|------|-------|
| `sound` | [SkillReference](#skillreference) | |

### ComBlock — `"COM"`

Command trigger.

| Field   | Type   | Notes |
|---------|--------|-------|
| `skill` | [SkillBlockReference](#skillblockreference) | |
| `time`  | byte   | |
| `steps` | [BlockCommandStep](#blockcommandstep)[] | |

### SCBlock — `"SC"`

| Field   | Type | Notes |
|---------|------|-------|
| `skill` | [SkillBlockReference](#skillblockreference) | |

### SFBlock — `"SF"`

| Field   | Type | Notes |
|---------|------|-------|
| `loop`  | byte | |
| `skill` | [SkillBlockReference](#skillblockreference) | |

### SGBlock — `"SG"`

| Field   | Type | Notes |
|---------|------|-------|
| `skill` | [SkillBlockReference](#skillblockreference) | |

### ColorBlock — `"COLOR"`

| Field      | Type | Notes |
|------------|------|-------|
| `option`   | int  | [ColorOption](#coloroption). |
| `rgba`     | [Rgba](#rgba) | |
| `aEnabled` | bool | *(derived)* `option == 4` (CustomAlpha). |

### EBBlock — `"EB"`

Screen effect / fade.

| Field        | Type   | Notes |
|--------------|--------|-------|
| `fadingType` | int    | [EBFadingType](#ebfadingtype). |
| `rgba`       | [Rgba](#rgba) | |
| `duration`   | ushort | |
| `player`     | bool   | |
| `enemy`      | bool   | |
| `bg`         | bool   | |
| `system`     | bool   | |
| `shakeBgX`   | [EBShakeBg](#ebshakebg) | |
| `shakeBgY`   | [EBShakeBg](#ebshakebg) | |

### AIBlock — `"AI"`

| Field        | Type | Notes |
|--------------|------|-------|
| `num`        | byte | |
| `time`       | byte | |
| `option`     | int  | [ColorOption](#coloroption). |
| `fadingType` | int  | [AIFadingType](#aifadingtype). |
| `rgba`       | [Rgba](#rgba) | |

### RBlock — `"R"`

Reaction routing. All fields are [SkillReference](#skillreference).

| Field           | Type | Notes |
|-----------------|------|-------|
| `hitsStand`     | SkillReference | |
| `hitsCrouched`  | SkillReference | |
| `hitsAir`       | SkillReference | |
| `guardStand`    | SkillReference | |
| `guardCrouched` | SkillReference | |
| `guardAir`      | SkillReference | |

### RCBlock — `"RC"`

Common-image reference.

| Field         | Type  | Notes |
|---------------|-------|-------|
| `out`         | bool  | *(derived)* `!in`. |
| `in`          | bool  | |
| `turnX`       | bool  | |
| `turnY`       | bool  | |
| `same`        | bool  | |
| `commonImage` | [SkillReference](#skillreference) | |
| `x`           | short | |
| `y`           | short | |

### RPBlock — `"RP"`

Hit-junction reference.

| Field         | Type  | Notes |
|---------------|-------|-------|
| `out`         | bool  | *(derived)* `!in`. |
| `in`          | bool  | |
| `turnX`       | bool  | |
| `hitJunction` | [SkillReference](#skillreference) | |
| `x`           | short | |
| `y`           | short | |

### GLBlock — `"GL"`

Life-gauge condition/change.

| Field    | Type  | Notes |
|----------|-------|-------|
| `skill`  | [SkillBlockReference](#skillblockreference) | |
| `isMore` | bool  | |
| `add`    | short | |

### GSBlock — `"GS"`

Special-gauge condition/change.

| Field    | Type  | Notes |
|----------|-------|-------|
| `skill`  | [SkillBlockReference](#skillblockreference) | |
| `isMore` | bool  | |
| `level`  | byte  | |
| `add`    | short | |

### GCBlock — `"GP"`

Gauge change. **Note:** the class is `GCBlock` but the emitted `type` string is `"GP"`.

| Field                | Type  | Notes |
|----------------------|-------|-------|
| `playerLifeGauge`    | short | |
| `playerSpecialGauge` | short | |
| `enemyLifeGauge`     | short | |
| `enemySpecialGauge`  | short | |

### RndBlock — `"Rnd"`

Random branch.

| Field          | Type   | Notes |
|----------------|--------|-------|
| `randomNum`    | ushort | |
| `whenItsAbove` | ushort | |
| `skill`        | [SkillBlockReference](#skillblockreference) | |

### DSBlock — `"DS"`

Do-skill-on condition.

| Field   | Type | Notes |
|---------|------|-------|
| `when`  | int  | [DSSkill](#dsskill). |
| `skill` | [SkillBlockReference](#skillblockreference) | |

### PSBlock — `"PS"`

| Field        | Type | Notes |
|--------------|------|-------|
| `playerTime` | byte | |
| `enemyTime`  | byte | |

### EBlock — `"E"`

End/terminator block. No fields beyond `index` and `type`.

### UnknownBlock — `"Unknown"`

Emitted when the parser encounters a block it does not recognize. No fields beyond
`index` and `type` (the raw bytes are held internally but not serialized). Treat its
presence as "unparsed data at this position".

---

## Nested command-step objects

### BlockCommandStep

Used by `ComBlock.steps`.

| Field      | Type | Notes |
|------------|------|-------|
| `direction`| int  | [ComDirection](#comdirection). |
| `a`        | bool | Button A. |
| `b`        | bool | Button B. |
| `c`        | bool | Button C. |
| `d`        | bool | Button D. |
| `e`        | bool | Button E. |
| `f`        | bool | Button F. |
| `continue` | bool | |
| `active`   | bool | |

### CommandStep

Used by `Command.steps`. Extends [BlockCommandStep](#blockcommandstep) (so it also has
`direction`, `a`–`f`, `continue`, `active`) and adds:

| Field    | Type   | Notes |
|----------|--------|-------|
| `type`   | int    | [CommandStepType](#commandsteptype). |
| `amount` | ushort | |

### CpuCommandStep

Used by `CpuCommand.steps`. Extends [CommandStep](#commandstep) and adds:

| Field     | Type | Notes |
|-----------|------|-------|
| `command` | [SkillReference](#skillreference) | |

---

## Per-file-type fields

Beyond the [envelope](#top-level-envelope-all-file-types), each file type adds:

### `game` (from a `.kgt`)

| Field             | Type     | Modes | Notes |
|-------------------|----------|-------|-------|
| `characters`      | string[] | both  | Character (`.player`) names. |
| `hitJunctions`    | string[] | both  | |
| `commonImages`    | string[] | both  | |
| `stages`          | string[] | both  | Stage (`.stage`) names. |
| `demos`           | string[] | both  | Demo (`.demo`) names. |
| `selectionScreen` | [SelectionScreenSettings](#selectionscreensettings) | both | |
| `baseSettings`    | [BaseSettings](#basesettings) | both | |
| `builtInSkills`   | [KGTBuiltInSkills](#kgtbuiltinskills) | both | |

### `character` (from a `.player`)

| Field                | Type     | Modes | Notes |
|----------------------|----------|-------|-------|
| `commands`           | [Command](#command)[] | both | |
| `settings`           | [PlayerSettings](#playersettings) | both | |
| `storyMode`          | [StoryMode](#storymode) | both | |
| `commonImages`       | [CommonImage](#commonimage)[] | both | |
| `cpu`                | [CpuCommand](#cpucommand)[] | both | CPU behavior commands. |
| `hitJunctionsSkills` | [HitJunctionSkills](#hitjunctionskills)[] | both | |
| `builtInSkills`      | [PlayerBuiltInSkills](#playerbuiltinskills) | both | |

### `stage` (from a `.stage`) and `demo` (from a `.demo`)

No fields beyond the [envelope](#top-level-envelope-all-file-types). They are
distinguished by `type` (`"stage"` / `"demo"`).

---

## Character sub-objects

### PlayerSettings

| Field             | Type | Notes |
|-------------------|------|-------|
| `age`             | int  | |
| `gender`          | int  | [Gender](#gender). |
| `sideHPYPos`      | ushort | |
| `interval`        | ushort | |
| `hRatio`          | byte | |
| `startPos`        | byte | |
| `correct`         | byte | |
| `combo`           | byte | |
| `guardButton`     | int  | [Button](#button). |
| `lifeGaugeMax`    | uint | |
| `specialGaugeMax` | uint | |
| `specialMaxStock` | uint | |
| `neutralGuard`    | bool | |
| `skyGuard`        | bool | |
| `guardWithButton` | bool | |
| `playerAttacks`   | short | |
| `enemyAttacks`    | short | |
| `startStock`      | uint | |

### Command

| Field           | Type   | Notes |
|-----------------|--------|-------|
| `name`          | string | |
| `time`          | uint   | |
| `airSkill`      | [SkillReference](#skillreference) | |
| `standSkill`    | [SkillReference](#skillreference) | |
| `standFarSkill` | [SkillReference](#skillreference) | |
| `crouchedSkill` | [SkillReference](#skillreference) | |
| `steps`         | [CommandStep](#commandstep)[] | |

### CpuCommand

| Field            | Type   | Notes |
|------------------|--------|-------|
| `name`           | string | |
| `probability`    | byte   | |
| `close`          | ushort | |
| `far`            | ushort | |
| `steps`          | [CpuCommandStep](#cpucommandstep)[] | |
| `characterInAir` | bool   | |
| `enemyInAir`     | bool   | |

### HitJunctionSkills

| Field         | Type | Notes |
|---------------|------|-------|
| `hitJunction` | [SkillReference](#skillreference) | |
| `spark`       | [SkillReference](#skillreference) | |

### PlayerBuiltInSkills

Maps built-in character states to skill indices. All fields are `ushort`:

`standing`, `forward`, `backward`, `jumpUp`, `frontJump`, `backJump`, `falling`,
`midCrouch`, `crouching`, `standFromCrouch`, `crouchAdvance`, `crouchRetreat`,
`turnStanding`, `turnCrouching`, `buttonGuardStand`, `buttonGuardCrouch`,
`buttonGuardAir`, `start`, `victory`, `loss`, `draw`, `charSelectPic`, `stageFacePic`,
`ri`.

### StoryMode

| Field     | Type                  | Notes |
|-----------|-----------------------|-------|
| `entries` | [StoryEntry](#story-entries)[] | Heterogeneous; discriminated by `type`. |

#### Story entries

Every entry has base fields `typeId` (byte) and `type` (string). The `type` string
selects the concrete shape:

**`"F"` — Fight entry**

| Field              | Type | Notes |
|--------------------|------|-------|
| `stage`            | [SkillReference](#skillreference) | |
| `numbOfRounds`     | byte | |
| `firstLife`        | int  | [StoryFirstLife](#storyfirstlife). |
| `lifeRecover`      | byte | |
| `ifDefeated`       | int  | [StoryIfDefeated](#storyifdefeated). |
| `startingRound`    | int  | [StoryStartingRound](#storystartinground). |
| `time`             | ushort | |
| `playerStartPos`   | uint | |
| `showRoundSkill`   | bool | |
| `showFightSkill`   | bool | |
| `wl`               | bool | |
| `ifTimeIsOverCpu`  | int  | [CPU](#cpu). |
| `ifTimeIsOverValue`| byte | |
| `cpuWinPoints`     | int  | [StoryCpuWinsPoints](#storycpuwinspoints). |
| `cpuWinPointsValue`| byte | |

**`"D"` — Demo entry**

| Field  | Type | Notes |
|--------|------|-------|
| `demo` | [SkillReference](#skillreference) | |

**`"J"` — Jump entry**

| Field       | Type | Notes |
|-------------|------|-------|
| `if`        | int  | [StoryEntryJump](#storyentryjump). |
| `value`     | byte | |
| `goToEvent` | any  | Usually `null`. |

**`"E"` — End entry** — no fields beyond `typeId` and `type`.

> Per-fight CPU opponent settings are parsed but **not currently emitted** in the JSON,
> so no corresponding field appears on the fight entry.

---

## KGT sub-objects

### SelectionScreenSettings

| Field                    | Type   | Notes |
|--------------------------|--------|-------|
| `charStartPosX`          | ushort | |
| `charStartPosY`          | ushort | |
| `distanceBetweenCharsX`  | ushort | |
| `distanceBetweenCharsY`  | ushort | |
| `columns`                | ushort | |
| `rows`                   | ushort | |
| `p1CursorPosX`           | ushort | |
| `p1CursorPosY`           | ushort | |
| `p1TeamBattleDiscanceX`  | short  | (source spelling: "Discance") |
| `p1TeamBattleDiscanceY`  | short  | |
| `p2CursorPosX`           | ushort | |
| `p2CursorPosY`           | ushort | |
| `p2TeamBattleDiscanceX`  | short  | |
| `p2TeamBattleDiscanceY`  | short  | |

### BaseSettings

| Field                          | Type | Notes |
|--------------------------------|------|-------|
| `offset`                       | bool | |
| `storyMode`                    | bool | |
| `vsMode`                       | bool | |
| `vsTeamMode`                   | bool | |
| `lockSource`                   | bool | |
| `numbersOnHPLifeBar`           | bool | |
| `cursorAppearsPressingAButton` | bool | |
| `stiffTime`                    | [StiffTime](#stifftime) | Object, not an enum. |

(An internal screen-select mapping exists in the model but is **not** serialized.)

### StiffTime

| Field    | Type | Notes |
|----------|------|-------|
| `hit`    | byte | |
| `guard`  | byte | |
| `offset` | byte | |

### KGTBuiltInSkills

Maps built-in system graphics/positions to skill indices. All fields are `uint`:

`none`, `hitLetterHit`, `hitNumber0`…`hitNumber9`, `offsetHitMark`, `roundAniStarttime`,
`roundAniEndtime`, `round1`…`round9`, `roundFinal`, `spirits`, `ko`, `perfect`,
`youWin`, `youLose`, `p1Wins`, `p2Wins`, `draw`, `doubleKo`, `unlimitedSign`,
`timeNumber0`…`timeNumber9`, `victoryMarkOn`, `victoryMarkOff`, `stageLayout1`…
`stageLayout10`, `p1LifeGauge`, `p2LifeGauge`, `p1SpecialGauge`, `p2SpecialGauge`,
`positionTimer`, `pos1pFace`, `pos2pFace`, `posSpecialStock1p`, `posSpecialStock2p`,
`posVictoryMark1p`, `vPosVictoryMark2p`, `titleCursor`, `positionForStoryMode`,
`positionForVsMode`, `continuteCursor`, `positionCursorItDoes`,
`positionCursorItDoesNot`, `p1VsScreenCursor`, `p2VsScreenCursor`,
`p1VsScreenCursorAfterInput`, `p2VsScreenCursorAfterInput`, `posCursorForTeamBattle`,
`pause`, `spare6`…`spare19`.

(Field spellings such as `continuteCursor` and `vPosVictoryMark2p` are preserved
verbatim from the source.)

---

## Enums

All enums serialize as **integers**. Tables below map value → name.

### SoundType
`0` None · `1` Wave · `2` Midi · `3` CDDA

### SettingsType
`0` None · `1` HitMark · `2` Time · `3` Position · `4` MarkPosition · `5` Character · `6` Stage

### HitMarkPosition
`0` Left · `1` Right

### ColorOption
`0` Normal · `1` Alpha50 · `2` Addition · `3` Subtraction · `4` CustomAlpha

### AIFadingType
`0` None · `1` Fixed · `2` Smooth · `3` Intermittent · `4` Random

### EBFadingType
`0` None · `1` Smooth · `2` Intermittent · `3` Random

### EBShakeBgType
`0` None · `1` FadingOut · `2` FadingIn · `3` Fixed · `4` Random

### DSSkill
`0` None · `1` Landing · `2` Attacking · `3` Defending · `4` WallHitting · `5` OffsetWay · `6` WhileThrowDo

### ComDirection
Declared `[Flags]` but with sequential values:
`0` Free · `1` Point · `2` Right · `3` DownRight · `4` Down · `5` DownLeft · `6` Left ·
`7` UpLeft · `8` Up · `9` UpRight · `10` UpLeftDown · `11` UpLeftRight · `12` UpRightDown ·
`13` DownLeftRight

### CommandStepType
`[Flags]` bitfield: `Press = 2` (`0b0010`) · `Repeat = 6` (`0b0110`) ·
`Charge = 10` (`0b1010`) · `Turn = 14` (`0b1110`)

### Gender
`0` Male · `1` Female · `2` Both · `3` None

### Button
`0` A · `1` B · `2` C · `3` D · `4` E · `5` F

### StoryFirstLife
`0` Recover · `1` CarryOver

### StoryIfDefeated
`0` None · `1` GameOver

### StoryStartingRound
`0` Zero · `1` PrevFight

### CPU
`0` CPU1 · `1` CPU2 · `2` CPU3 · `3` CPU4 · `4` CPU5 · `5` CPU6 · `6` CPU7

### StoryCpuWinsPoints
`0` LastGivenAttack · `1` Player · `2` CPU1 · `3` CPU2 · `4` CPU3 · `5` CPU4 · `6` CPU5 ·
`7` CPU6 · `8` CPU7

### StoryEntryJump
`0` None · `1` LoseInFrontIfST · `2` HaveLittleLifeInPreviousFight · `3` WinAllFights

### EBShakeBg

Not an enum but a small object used by `EBBlock.shakeBgX` / `shakeBgY`:

| Field      | Type | Notes |
|------------|------|-------|
| `type`     | int  | [EBShakeBgType](#ebshakebgtype). |
| `shake`    | byte | |
| `duration` | byte | |
