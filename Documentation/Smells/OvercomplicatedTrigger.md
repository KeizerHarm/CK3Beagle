# Overcomplicated Trigger
This is when a trigger block grows more complicated than it needs to be. Triggers that stack multiple layers of ANDs and ORs can often be rewritten to be flatter, and have less duplicated checking for the same triggers, making understanding and maintaining them easier.

## OT.1: [Associativity](https://en.wikipedia.org/wiki/Associative_property)
ANDs are true if all members are true, ORs are true if one of the members are true. An AND with as a child a second AND, or an OR with a second OR, is written redundantly; the members of the child trigger can be added to the parent.

<table>
  <thead>
    <tr>
      <th>Original</th>
      <th>Refactored</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><pre>
AND = {
   has_trait = zealous
   AND = {
      has_trait = arbitrary
      has_trait = gluttonous
   }
}
</pre></td>
      <td><pre>
AND = {
   has_trait = zealous
   has_trait = arbitrary
   has_trait = gluttonous
}
</pre></td>
    </tr>
    <tr>
      <td><pre>
OR = {
   gold > 250
   OR = {
      piety > 100
      prestige <= 2500
   }
}
</pre></td>
      <td><pre>
OR = {
   gold > 250
   piety > 100
   prestige <= 2500
}
</pre></td>
    </tr>
  </tbody>
</table>


## OT.2: [Idempotence](https://en.wikipedia.org/wiki/Idempotence)
This is simply the same trigger line appearing twice in the same block. Checking the same trigger twice is not going to give you different results. The repetition is redundant and may be confusing.

<table>
  <thead>
    <tr>
      <th>Original</th>
      <th>Refactored</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><pre>
AND = {
   has_trait = zealous
   has_trait = arbitrary
   has_trait = zealous
}
</pre></td>
      <td><pre>
AND = {
   has_trait = zealous
   has_trait = arbitrary
}
</pre></td>
    </tr>
  </tbody>
</table>


## OT.3: [Contradiction](https://en.wikipedia.org/wiki/Law_of_noncontradiction)
This is the mirror image of idempotence; when you check for the trigger, and its opposite, in the same block. They cannot possibly be both true, so this is equivalent to writing `always = no` in the block. Whichever line is incorrect should be removed for this to make sense.

<table>
  <thead>
    <tr>
      <th>Original</th>
      <th>Refactored</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><pre>
AND = {
   has_trait = zealous
   has_trait = arbitrary
   NOT = { has_trait = zealous }
}
</pre></td>
      <td><pre>
AND = {
   has_trait = zealous
   has_trait = arbitrary
   NOT = { has_trait = zealous }
}
</pre></td>
    </tr>
    <tr>
      <td><pre>
OR = {
   faith = faith:catholic
   piety > 2500
   faith != faith:catholic
}
</pre></td>
      <td><pre>
OR = {
   faith = faith:catholic
   piety > 2500
}
</pre></td>
    </tr>
    <tr>
      <td><pre>
AND = {
   my_scripted_trigger = no
   my_other_scripted_trigger = yes
   my_scripted_trigger = yes
}
</pre></td>
      <td><pre>
AND = {
   my_scripted_trigger = no
   my_other_scripted_trigger = yes
}
</pre></td>
    </tr>
  </tbody>
</table>


## OT.4: [Double Negation](https://en.wikipedia.org/wiki/Double_negation)
Some people don't take no for an answer, but for computers, saying no twice flips it back to a yes. It is confusing and often redundant to wrap a negative statement (a `some_trigger = no`, `link != value`, or `NOT/NOR/NAND = { something = yes }`) into another NOT or NOR (but not NAND). When you take this to the next level and negate _all_ children of a NOR, you can flip it into an AND with the inverses; likewise with a NAND to a NOR.

<table>
  <thead>
    <tr>
      <th>Original</th>
      <th>Refactored</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><pre>
NOT = {
   NOT = { gold > 2500 }
}
</pre></td>
      <td><pre>
gold > 2500
</pre></td>
    </tr>
    <tr>
      <td><pre>
NOR = {
   my_scripted_trigger = no
}
</pre></td>
      <td><pre>
my_scripted_trigger = yes
</pre></td>
    </tr>
    <tr>
      <td><pre>
NOR = {
   NOT = { has_trait = zealous }
   faith != faith:catholic
   my_trigger = no
}
</pre></td>
      <td><pre>
AND = {
   has_trait = zealous
   faith = faith:catholic
   my_trigger = yes
}
</pre></td>
    </tr>
    <tr>
      <td><pre>
NAND = {
   faith != faith:fire_spirituality
   NOT = { has_primary_title = title:e_fire_nation }
}
</pre></td>
      <td><pre>
OR = {
   faith = faith:fire_spirituality
   has_primary_title = title:e_fire_nation
}
</pre></td>
    </tr>
  </tbody>
</table>

## OT.5: [Distributivity](https://en.wikipedia.org/wiki/Distributive_property#Propositional_logic)
It is common to want to describe multiple different cases in a long trigger; structuring a block like an OR with many AND members. But when each of those ANDs share a trigger (a grandchild of the original OR), when you have a repetition among all of your cases, then the entire trigger can be rewritten more compactly to check for that trigger and _then_ one of all your cases. The inverse, a shared trigger between every OR-block in an AND, is less common but the rule works the same.

<table>
  <thead>
    <tr>
      <th>Original</th>
      <th>Refactored</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><pre>
OR = {
    AND = {
        has_trait = zealous
        is_clergy = yes
    }
    AND = {
        has_trait = zealous
        has_primary_title = title:k_papacy
    }
    AND = {
        has_trait = zealous
        piety > 2500
    }
}
</pre></td>
      <td><pre>
AND = {
    has_trait = zealous
    OR = {
        is_clergy = yes
        has_primary_title = title:k_papacy
        piety > 2500
    }
}
</pre></td>
    </tr>
    <tr>
      <td><pre>
AND = {
    OR = {
        stress_level >= 1
        num_sinful_traits > 1
    }
    OR = {
        stress_level >= 1
        NOT = { has_trait = pilgrim }
    }
}
</pre></td>
      <td><pre>
OR = {
    stress_level >= 1
    AND = {
        num_sinful_traits > 1
        NOT = { has_trait = pilgrim }
    }
}
</pre></td>
    </tr>
  </tbody>
</table>



## OT.6: [Absorption](https://en.wikipedia.org/wiki/Absorption_law)
This describes another case of redundant repetition. An OR with an AND that contains a trigger that's already in the parent OR, is checking the same thing twice and without changing the truth of the whole statement. Move the condition up to the parent OR to simplify the trigger. This also applies for the inverse with OR and AND swapped.

<table>
  <thead>
    <tr>
      <th>Original</th>
      <th>Refactored</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><pre>
OR = {
    my_scripted_trigger = yes
    AND = {
        has_trait = arbitrary
        my_scripted_trigger = yes
    }
}
</pre></td>
      <td><pre>
OR = {
    my_scripted_trigger = yes
    has_trait = arbitrary
}
</pre></td>
    </tr>
    <tr>
      <td><pre>
AND = {
    OR = {
        faith = faith:catholic
        gold < 250
    }
    faith = faith:catholic
}
</pre></td>
      <td><pre>
AND = {
    faith = faith:catholic
    gold < 250
}
</pre></td>
    </tr>
  </tbody>
</table>


<sub>[Highly illogical!](https://www.youtube.com/watch?v=Ru9e2rTHeuk)</sub>
