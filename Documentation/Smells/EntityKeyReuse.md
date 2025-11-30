# Not is Not Nor

It is possible to reuse keys for entity declarations; the CK3 architecture supports overrides of individual entities. But if that happens within a project, then careless copy-pasting is usually at fault, and the consequence is that someone working on one version is unaware that the second version is overriding it.

It is also possible to reuse an entity key, for entities of different types. This is also something supported in CK3 architecture but it may lead to confusion, and as such it is discouraged as well.

## Configuration

Only the respective Severities of the two cases are configurable.

## `EKR.1`: Same Type
Reusing the same key for another entity of the same type, means the earlier declaration is overruled. In the example below, the first `establish_bon_hof_decision` will not be used by the game.
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
establish_bon_hof_decision = {
    #First implementation
}
</br>
#Later, in the same file, or in a different file in the same mod
establish_bon_hof_decision = {
    #New implementation
}
</pre></td>
      <td><pre>
establish_bon_hof_decision = {
    #First implementation
}
</br>
#Later, in same file, or in a different file in the same mod
establish_bon_hof_decision_v2 = {
    #New implementation
}
</pre></td>
    </tr>
  </tbody>
</table>

## `EKR.2`: Different Type
Reusing the same key for another entity of a different type, does not make for a technical issue, but it is still one in usability. In the example below, `my_thing` works as _both_ a scripted trigger and a scripted effect. The fact that we have to call it 'thing' says that maybe the two should have different names. Try suffixing or prefixing with the scope type, like vanilla.

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
scripted_effect my_thing = {
    add_gold = 450
}
</br>
scripted_trigger my_thing = {
    gold >= 450
}
</pre></td>
      <td><pre>
scripted_effect my_thing_effect = {
    add_gold = 450
}
</br>
scripted_trigger my_thing_trigger = {
    gold >= 450
}
</pre></td>
    </tr>
  </tbody>
</table>