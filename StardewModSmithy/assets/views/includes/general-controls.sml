<frame layout="stretch content" padding="30,20" border={@Mods/StardewUI/Sprites/ControlBorder}>
  <lane orientation="vertical">
    <panel *if={TextureHasAtlas} layout="stretch content" horizontal-content-alignment="start">
      <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
        layout="stretch 56px"
        margin="0,4,0,0"
        text={#gui.button.populate}
        left-click=|PopulateFromAtlas()|
      />
    </panel>
    <include name="mushymato.StardewModSmithy/views/includes/bounds-provider-selector" *context={:BoundsProviderSelector}/>
    <grid layout="stretch content" item-layout="count: 2" item-spacing="4,4" >
      <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
        layout="stretch 56px"
        text={#gui.button.create}
        left-click=|Create()|
      />
      <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
        layout="stretch 56px"
        text={#gui.button.delete}
        left-click=|Delete()|
      />
      <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
        layout="stretch 56px"
        text={#gui.button.save}
        left-click=|Save()|
      />
      <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
        layout="stretch 56px"
        text={#gui.button.exit}
        left-click=|Exit()|
      />
    </grid>
    <label layout="stretch content" opacity="0.6" text={LastSavedMessage}/>
  </lane>
</frame>
