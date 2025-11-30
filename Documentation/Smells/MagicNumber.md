# Magic Number

This is another problem with clarity and duplication. `add_gold = 150` is considered problematic for three major reasons:

* It doesn't actually convey what the number is supposed to be. Is 150 gold meant to be a lot in this circumstance? Or just a little?
* If this value needs to be changed (for example if the income is rebalanced) it can be hard to track down every instance and arduous to change; a central definition would be much easier to tweak.
* It does not take into account context, such as cost scaling over game eras.

The solution to all three problems is to use a script value instead.

Now in regular programming, any instance of a number is considered a problem. For the CK3 script, only numbers used as function arguments need to be considered. That's because the other numbers, such as chance weighting in `random_lists`, do not have those problems: their meanings are clear, changing will (almost always) be done with localised rebalancing, and they don't necessarily need to change from context. So `prestige > 150` is considered smelly, `chance = 15` in a random block isn't.

## Configuration
Besides Severity, you can manage the exact list of triggers and effects which ought to use script values instead of number literals. By default, the triggers and effects are those referencing spendable currencies.

## `MN.1`: Magic Number
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
add_gold = 150
</pre></td>
      <td><pre>
add_gold = medium_gold_value
</pre></td>
    </tr>
  </tbody>
</table>