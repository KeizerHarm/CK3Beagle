# Keyword As Scope Name
CK3 doesn't reserve words, any alphanumeric string can be used as a name for a saved scope. However, when you're trying to redefine a special link, then you're either being negligently unclear or perhaps unsure how to use the link for its intended purpose.

## Configuration
The only configuration is the respective Severities of the two cases.

## `KSN.1`: Root or Prev
`root` and `prev` are predefined scope links, but the way they work is a bit special; linking to the origin scope and the previous scope respectively. Some new modders are not sure how to use them, and try to save scopes under their names. That's meaningless and confusing, use any name but that.

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
random_targeting_scheme = {
	scheme_owner = {
		save_scope_as = root
		#This looks like it attempts to change the root, but it just saves a scope called root
	}
}
</pre></td>
      <td><pre>
random_targeting_scheme = {
	scheme_owner = {
		save_scope_as = targeting_schemer
	}
}
</pre></td>
    </tr>
  </tbody>
</table>

## `KSN.2`: Scope Link
More common than the egregious `root`/`prev` is using generic names for scopes, so generic in fact that they are already used for regular scope links. That includes `father`, `primary_heir`, `capital_province`, but also `faith` and `scope` itself. The full list is exactly the scopes in `event_targets.log`.

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
father = {
	save_temporary_scope_as = father
}
</pre></td>
      <td><pre>
father = {
	save_temporary_scope_as = schemers_father
}
</pre></td>
    </tr>
  </tbody>
</table>