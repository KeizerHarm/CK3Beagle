# Unencapsulated Addition
The wisdom of twenty-five years of PDX modding can be summed up in five words: change as little as possible. Not only in terms of impact, but in terms of literal line count.

We have macros to encapsulate many changes into one. Let's make good use of them! This rule is to minimise your footprint; when adding many lines to any block of trigger or effect script, that addition should be extracted to a macro, so that you can pack the same punch in addition of only one line. This also makes compatibility with other mods easier, as those macros can be overridden individually, à la carte.

## Configuration
The only configuration is the Severity setting.

## `UA.1`: Unencapsulated Addition
This is a Diff Smell; the example is a pair of a vanilla script and the mod's edit.

<table>
  <thead>
    <tr>
      <th>Vanilla</th>
      <th>Mod (Smelly)</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><pre>
start_cat_story_cycle_effect = {
    show_as_tooltip = {
        add_character_modifier = {
            modifier = cat_story_modifier
        }
    }
    hidden_effect = {
        create_story = story_cycle_pet_cat
    }
}
</pre></td>
      <td><pre>
start_cat_story_cycle_effect = {
    show_as_tooltip = {
        add_character_modifier = {
            modifier = cat_story_modifier
        }
    }
    hidden_effect = {
        create_story = story_cycle_pet_cat
    }
    <span style="background: green;">
    add_stress = monumental_stress_loss
    remove_trait = melancholic
    remove_trait = irritable
    remove_trait = dog_lover
    add_trait = cat_lover</span>
}
</pre></td>
    </tr>
    <tr>
      <th>Vanilla</th>
      <th>Mod (Refactored)</th>
    </tr>
    <tr>
      <td><pre>
start_cat_story_cycle_effect = {
    show_as_tooltip = {
        add_character_modifier = {
            modifier = cat_story_modifier
        }
    }
    hidden_effect = {
        create_story = story_cycle_pet_cat
    }
}
</pre></td>
      <td><pre>
start_cat_story_cycle_effect = {
    show_as_tooltip = {
        add_character_modifier = {
            modifier = cat_story_modifier
        }
    }
    hidden_effect = {
        create_story = story_cycle_pet_cat
    }
    <span style="background: green;">
    petmod_start_cat_story_effect = yes</span>
}
<span style="background: green;">
#In the mod's file
petmod_start_cat_story_effect = {
    add_stress = monumental_stress_loss
    remove_trait = melancholic
    remove_trait = irritable
    remove_trait = dog_lover
    remove_trait = cat_lover
}</span>
</pre></td>
    </tr>
  </tbody>
</table>