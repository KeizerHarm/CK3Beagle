# Not is Not Nor

The CK3 script supports a healthy amount of boolean algebra, and some extensions like `trigger_if`, but the one place where it flies in the face of convention of programmers and logicians alike, is in its support of `NOT` with multiple children. This was a logical necessity in earlier games, where `NOR` was pre-assigned to the nation of Norway. But in Crusader Kings III the `NOR` and `NAND` are available and should be used where proper. NOT should only be used to negate a single child trigger, which is all that the function has a standardised definition for.

## Configuration

Only the Severity is configurable.

## `NNN.1`: Not is Not Nor
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
    is_adult = yes
    piety >= medium_piety_value
}
</pre></td>
      <td><pre>
NOR = {
    is_adult = yes
    piety >= medium_piety_value
}
</pre></td>
    </tr>
  </tbody>
</table>