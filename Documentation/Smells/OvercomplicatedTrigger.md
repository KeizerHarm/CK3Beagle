# Overcomplicated Trigger
This is when a trigger block grows more complicated than it needs to do a given check. Triggers that stack multiple layers of ANDs and ORs can often be rewritten to be flatter, and have less duplicated checking for the same triggers, making comprehending and maintaining them easier.

This smell has many different forms. Examples are given for each.

## Associativity
ANDs are true if all members are true, ORs are true if one of the members are true. An AND with as a child a second AND, or an OR with a second OR, is written [redundantly](https://en.wikipedia.org/wiki/Associative_property); the members of the child trigger can be added to the parent.

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
      <td>```
AND = {
   has_trait = zealous
   has_trait = arbitrary
   has_trait = gluttonous
}
```</td>
    </tr>
  </tbody>
</table>