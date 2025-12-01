# Terminology

<table>
  <thead>
    <tr>
      <th>Term</th>
      <th>Meaning</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>Trigger</td>
      <td>Any simple triggers, complex triggers, or links in a trigger context.</td>
    </tr>
    <tr>
      <td>Effect</td>
      <td>Any simple effects, complex effects, or links in an effect context.</td>
    </tr>
    <tr>
      <td>Statement</td>
      <td>Any independently reusable line of script - including effects, triggers, links.<br/>
        In an event option, <code>name</code> is not a statement but <code>add_gold = 50</code> is; as is the whole block <code>save_scope_value_as = { name = aaa value = xxx }</code> as a whole, and not <code>name = aaa</code> or <code>value = xxx</code>.<br/>
        Statements are used to calculate the size of units, and only statements are considered for Duplication.</td>
    </tr>
    <tr>
      <td>Declaration</td>
      <td>Any block at the top level that defines a new entity. Events and cultures are declarations, but faiths are not, as they are declared with their religions.
    </tr>
    <tr>
      <td>Macro</td>
      <td>Any declaration that contains only reusable script, and can be treated as reusable itself. The four macro types as of time of writing are scripted triggers, scripted effects, scripted modifiers and script values.
    </tr>
  </tbody>
</table>
