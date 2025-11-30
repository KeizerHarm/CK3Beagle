# Hidden Dependencies

Even the best scripted effect is no match for a careless modder (or content designer) invoking it in the wrong place, from the wrong scope or in an environment lacking the prerequisite setup. So to be safest, a macro should either avoid having any implicit needs, or should document its requirements in a comment (serving as documentation).

## Configuration
This smell is highly configurable. Each of the cases below can be given their own Severity, and also their Remedies.

### Remedies
If something is forbidden (like the use of `root`), when can it be allowed?

#### Allowed if in Name
If the word `root`, `prev`, the scope's name or the variable's name, are part of the macro's name, they will be allowed.
<pre>
pope_likes_root = {
	title:k_papacy.holder = {
		opinion = {
			target = root
			value > 60
		}
	}
}
</pre>

#### Allowed if in Comment
If the word `root`, `prev`, the scope's name or the variable's name, are part of a (documentation) comment preceding the macro, they can be allowed.
<pre>
#Uses root
pope_likes_you = {
	title:k_papacy.holder = {
		opinion = {
			target = root
			value > 60
		}
	}
}
</pre>

#### Allowed if in Event File
Scripted triggers/effects that are defined in event files are physically closer to where they are used, and cannot be used elsewhere. One can be more lenient with them.
<pre>
scripted_trigger pope_likes_you = {
	title:k_papacy.holder = {
		opinion = {
			target = root
			value > 60
		}
	}
}
</pre>

#### Allowed if in White List
Only relevant for the cases of Saved Scopes and Variables. You can document lists of scope names and variable names that you trust to be properly set up everywhere they are called from. In the example below, if we add `dragonborn` to the whitelist for saved scopes, the smell will not trigger.
<pre>
scripted_trigger pope_likes_dragonborn = {
	title:k_papacy.holder = {
		opinion = {
			target = scope:dragonborn
			value > 60
		}
	}
}
</pre>

## `HD.1`: Use of Root
`root` is a link to the scope for which the current effect chain started; for example, the character for which the event fired. As such, a macro using root is dependend on a larger context than just where it is invoked. Refactor, or use a parameter instead.
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
has_reason_to_be_paranoid_trigger = {
	any_relation = {
		type = friend
		has_de_jure_claim_on = root 
		#May not be the scope this trigger runs from
	}
}
</pre></td>
	  <td><pre>
has_reason_to_be_paranoid_trigger = {
	any_relation = {
		type = friend
		has_de_jure_claim_on = prev  
		#Will be the exact scope this trigger runs from, this is fine
	}
}
</pre></td>
	</tr>
  </tbody>
</table>

## `HD.2`: Use of Top-Level Prev
`prev` is a link to the *previous* scope; it is going to reflect back to where you *just* came from. If a macro uses `prev` to go back to a scope it itself entered earlier, that's fine, but if it uses `prev` at the *top* level - before any scope changes - then it is going to some scope entered outside of its context. That makes it dependent on where it was invoked. Use a macro instead.
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
hates_my_guts_trigger = {
	opinion = {
		target = prev 
		#This prev is at the top level, it was used before any scope change
		value < -50
	}
}
</pre></td>
	  <td><pre>
hates_my_guts_trigger = {
	opinion = {
		target = $HATED$
		#Refactored to parameter
		value < -50
	}
}
</pre></td>
	</tr>
  </tbody>
</table>

## `HD.3`: Use of Saved Scope
Saved scopes, `scope:scopename`, are set at some point in the preceding effect chain with a `save_scope_as` effect, or sometimes they are provided by code. A scripted trigger or effect using these relies on that to have gone well. A parameter refactor makes the macro independent, and also more flexible - if you have the scope available in a context but saved under a different name, you don't have to re-save it under the name the macro needs.

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
has_reason_to_be_paranoid_trigger = {
	scope:vengeful_lover = { #Use of saved scope
		gold > medium_gold_value
	}
}
</pre></td>
	  <td><pre>
has_reason_to_be_paranoid_trigger = {
	$VENGEFUL_LOVER$ = { #A parameter is used instead
		gold > medium_gold_value
	}
}
</pre></td>
	</tr>
  </tbody>
</table>

Besides the configurable remedies, this can also be remedied by the macro checking for the scope's existence before doing anything.
<pre>
if = {
	limit = {
		exists = scope:scopename
	}
	...
}
</pre>

## `HD.4`: Use of Variable
A variable, (`var:varname`, `global_var:varname` or `local_var:varname`), can be set by any part of the game, meaning it is sometimes hard to *guarantee* that it does. A scripted trigger/effect should not carelessly assume that it did.

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
the_seven_kingdoms_are_relieved_trigger = {
	global_var:night_king_is_dead = yes
}
</pre></td>
	  <td><pre>
the_seven_kingdoms_are_relieved_trigger = {
	exists = global_var:night_king_is_dead
}
</pre></td>
	</tr>
  </tbody>
</table>

Besides the configurable remedies, this can also be remedied by the macro checking for the variable's existence before doing anything.
<pre>
if = {
	limit = {
		exists = var:varname
	}
	...
}
</pre>
