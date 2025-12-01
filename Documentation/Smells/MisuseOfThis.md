# Misuse of This
`this` is an unusual scope link; in that it goes nowhere. If the other scope links are doorways, `this` is a mirror, used when you need a reference to your present scope. To try to enter it using `this = {` is never sensible. It takes you nowhere!

Note that `this ?= scope:somescope`, is not considered a misuse. That is because there are contexts where `this` is not guaranteed to exist: there are places where you need to check `exists = this`.

## Configuration
Only the Severity of this smell is configurable.

## MT.1
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
scope:bad_vassal = {
    remove_short_term_gold = medium_gold_value
    this = {
        death = { death_reason = death_vanished }
    }
}
</pre></td>
      <td><pre>
scope:bad_vassal = {
    remove_short_term_gold = medium_gold_value
    death = { death_reason = death_vanished }
}
</pre></td>
    </tr>
  </tbody>
</table>