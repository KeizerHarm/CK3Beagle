# Large Unit

Script is duplicated when it appears twice or more, doing the same thing to something. It poses an obvious problem for maintenance; if two events in two different contexts need to ensure they are dealing with a faith that forbids alcohol, and they use the same hard-written enumeration of faiths for it, then whenever an expansion (or a mod) adds another teetotaler religion, that implementer will need to find all of those triggers and update all of them. Good luck keeping that straight! Extracting the duplicated parts to macros is how you prevent duplication and ease maintenance a whole lot.

Note that this refers only to duplication of generic, reusable script; such as trigger and effect script - stuff you could theoretically reuse. This does not refer to the lines that are part of declarations like all the god names of a religion definition, which cannot be extracted to macros anyway.

## Configuration
Besides severity, you can also configure the Minimum Size; at how many copied statements is repetition a problem?

## DUP.1: Duplication
<table>
  <thead>
    <tr>
      <th>Original</th>
      <th>Refactored</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><pre>brutally_murder_pope_effect = {
    title:k_papal_state.holder = {
        death = {
            death_reason = death_cloven_in_half
            killer = $MURDERER$
        }
        faith = {
            every_faith_playable_ruler = {
                #Various effects informing them of the death
                #[...]
            }
        }
    }
}

brutally_murder_patriarch_effect = {
    title:k_orthodox.holder = {
        death = {
            death_reason = death_cloven_in_half
            killer = $MURDERER$
        }
        faith = {
            every_faith_playable_ruler = {
                #Various effects informing them of the death
                #[...]
            }
        }
    }
}
</pre></td>
      <td><pre>brutally_murder_christian_hof_effect = {
	death = {
		death_reason = death_cloven_in_half
		killer = $MURDERER$
	}
	faith = {
		every_faith_playable_ruler = {
			#Various effects informing them of the death
			#[...]
		}
	}
}

brutally_murder_pope_effect = {
	title:k_papal_state.holder = {
		brutally_murder_christian_hof_effect = {
			MURDERER = $MURDERER$
		}
	}
}

brutally_murder_patriarch_effect = {
	title:k_orthodox.holder = {
		brutally_murder_christian_hof_effect = {
			MURDERER = $MURDERER$
		}
	}
}
</pre></td>
    </tr>
  </tbody>
</table>


