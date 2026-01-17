# Design Principles for Realistic Terrain

Understanding **why** natural landscapes look convincing helps us write better procedural algorithms.

## The Core Idea

Nature follows rules. By observing and encoding these rules into algorithms, we can generate convincing terrain **without artistic talent**. The algorithms aren't "creative" - they're applying patterns observed in nature.

---

## The 5 Design Principles

### 1. Contrast

**What it is:** Differences that draw attention and create visual interest

**In nature:**
- Steep cliffs vs. soft clouds
- Bright sand vs. dark jungle
- White snow vs. green pines
- Flat plains vs. mountains (creates focal points)

**For our code:** Generate varied terrain - don't make everything the same height/color

---

### 2. Repetition

**What it is:** Consistent reuse of elements for coherence

**In nature:**
- Same tree species grouped together
- Climate zones have consistent vegetation
- Predictable patterns (no cacti in rainforests)

**Benefits:**
- Feels believable to viewers
- Reduces processing (reuse same models/textures)
- Easier for brain to process

**For our code:** Use limited asset types, repeat them appropriately

**Key insight:** "Less is more" - too many different objects = cluttered, not rich

---

### 3. Alignment

**What it is:** Purposeful positioning based on rules

**In nature:**
- Trees grow at specific altitudes
- Certain species need specific soil/water conditions
- Same types clump together
- Vegetation follows terrain features

**For our code:** Place trees by height bands, grass near water, rocks on slopes

---

### 4. Proximity

**What it is:** Related elements placed close together

**In nature:**
- Similar trees grow in groups (forests, not scattered individuals)
- Ferns and rocks near water
- Grass doesn't grow under large trees (shade + water competition)

**For our code:** Group similar vegetation, consider what grows near what

---

### 5. Coherence

**What it is:** Everything looks like it belongs together

**How to achieve it:**
- Use assets from the same artist/pack
- Consistent color palette
- Consistent style across all elements

**Warning:** Mixing assets from different artists breaks coherence - their styles clash

**Pro tip:** Don't trust your memory for colors - use reference photos and sample actual colors. Real grass/sky/rock colors are often surprising.

---

## Summary Table

| Principle | Rule for Procedural Generation |
|-----------|-------------------------------|
| **Contrast** | Vary heights, create focal points, mix colors |
| **Repetition** | Limit asset variety, repeat consistently |
| **Alignment** | Place by altitude, slope, distance to water |
| **Proximity** | Group similar objects, consider neighbors |
| **Coherence** | Use matching asset sets, consistent palette |

---

## Application in Code

Every algorithm we write will encode these principles:

- **Height-based tree placement** → Alignment
- **Noise functions** → Natural repetition with variation
- **Texture blending by slope/height** → Contrast + Alignment
- **Vegetation rules** (what grows where) → Proximity

---

## References

- Just Cause 2 - Beach/jungle contrast example
- Skyrim - Repetition and alignment of vegetation
- Real-world photography for color sampling
