// ${COMPLETE_ITEM:IsNullOrEmpty}

class Foo {
  public void M(System.Action f, string str, int[] xs) {
    f(() => {
      foreach (var x in xs) {
        var smth = x + 1;
        str.{caret}
      }
    });
  }
}