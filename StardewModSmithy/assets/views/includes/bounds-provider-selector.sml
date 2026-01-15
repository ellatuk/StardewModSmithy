<lane orientation="vertical"
  layout="stretch content[..1000]"
  vertical-content-alignment="middle"
  margin="4,4">
  <lane orientation="horizontal" vertical-content-alignment="middle" padding="0,16">
    <image sprite={@Mods/StardewUI/Sprites/CaretLeft} focusable="true"
        left-click=|Decrease()|
        +hover:scale="1.2"
        +transition:scale="100ms EaseOutCubic"/>
    <label text={ValueLabel}
        tooltip={ValueLabel}
        font="dialogue"
        layout="stretch content"
        padding="20,0"
        focusable="true"
        horizontal-alignment="middle"
        shadow-alpha="0.8"
        shadow-color="#4448"
        shadow-offset="-2, 2"
        wheel=|Wheel($Direction)|
        left-click=|ToggleViewingBoundsProviderList()|/>
    <image sprite={@Mods/StardewUI/Sprites/CaretRight} focusable="true"
        left-click=|Increase()|
        +hover:scale="1.2"
        +transition:scale="100ms EaseOutCubic"/>
  </lane>
  <textinput *if={ViewingBoundsProviderList}
  text={<>BoundsProviderSearchTerm}
  placeholder={#gui.placeholder.search}
  font="dialogue" layout="content 64px" margin="-4,0,0,0"/>
  <scrollable *if={ViewingBoundsProviderList} peeking="128" scrollbar-margin="8,0,0,0">
    <lane orientation="vertical" layout="content content">
      <frame *repeat={FilteredBoundsProviderList}
      border={@Mods/StardewUI/Sprites/MenuSlotTransparent}
      border-thickness="4"
      layout="stretch content"
      margin="2"
      padding="8"
      background={@Mods/StardewUI/Sprites/White}
      background-tint="Transparent"
      +state:selected={:IsSelected}
      +state:selected:background-tint="#39d"
      +hover:background-tint="Wheat"
      left-click=|^SelectBoundsProvider(this)|>
      <label text={:UILabel}
          tooltip={:UILabel}
          font="small"
          focusable="true"
          horizontal-alignment="start"
          shadow-alpha="0.8"
          shadow-color="#4448"
          shadow-offset="-2, 2"
          max-lines="1"/>
      </frame>
    </lane>
  </scrollable>
</lane>
