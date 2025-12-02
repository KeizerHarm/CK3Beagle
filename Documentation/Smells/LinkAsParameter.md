# Link as Parameter

A scope link takes you from one scope to another, like `father` to a character's dad. When a link is passed as a macro parameter, we might expect the link to be parsed from the context where the macro is invoked. In reality, the macro does what it wants with the argument, possibly using it in a different context entirely. This behaviour is very prone to causing errors, so the best practice is to never pass a link as a macro argument.

<pre>
#Scripted effect
#Makes someone hated in the court where the effect is run
make_pariah_in_court_effect = {
	every_courtier = {
		add_opinion = {
			modifier = hate
			target = $PARIAH$
		}
	}
}

#Careless invocation; we might assume that this makes the HRE holder's father the pariah
title:e_hre.holder = {
	make_pariah_in_court_effect = { PARIAH = father }
}

#But because of text substitution, this is the resultant effect
# every_courtier = {
#	 add_opinion = {
#		 modifier = hate
#		 target = father
#	 }
# }
# Meaning every courtier hates their own father
</pre>

While it is better to never pass a scope link to begin with, two kinds of scripted effects/triggers are allowed to receive links and won't count for the smell. The first is one that immediately saves that parameter as a scope, and from then on uses only the scope. That fixes the reference in place from where the macro is invoked.
<pre>
make_pariah_in_court_effect = {
	$PARIAH$ = {
		#Fine. save_scope_as is also allowed.
		save_temporary_scope_as = pariah
	}
	every_courtier = {
		add_opinion = {
			modifier = hate
			target = scope:pariah
		}
	}
}
</pre>

The second is a macro that never uses the parameter in isolation, as its own token. For example, it accesses a scope by the passed name, saves the parameter as a flag, or concatenates it to a longer token.
<pre>
my_effect = {
	save_scope_value_as = {
		name = casename
		value = flag:$CASE$ #Fine
	}

	scope:$CASE$ = { add_gold = 50 } #Fine
	
	$CASE$_effect = yes #Fine
}
</pre>

## Configuration
Only the Severity of this smell may be configured.

## `LP.1`: Link as Parameter
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
make_pariah_in_court_effect = { PARIAH = father }
</pre></td>
      <td><pre>
father = { save_scope_as = rulers_father }
make_pariah_in_court_effect = { PARIAH = scope:rulers_father }
</pre></td>
    </tr>
  </tbody>
</table>