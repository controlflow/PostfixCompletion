// ${COMPLETE_ITEM:Foo}

[System.Flags]
public enum CoolFlags
{
  None,
  Foo = 1 << 1,
  Bar = 1 << 2,
  Boo = Foo | Bar
}

class Foo
{
  public CoolFlags MyFlags;
  public bool M(Foo foo) => foo?.MyFlags.{caret}
}