p=r"reversed\rea\scripts\audit_official_impl.py"
s=open(p,encoding="utf-8").read()
s=s.replace('r"public IEnumerator PlayVOIfLethal",', 'r"private IEnumerator PlayVOIfLethal",')
open(p,"w",encoding="utf-8").write(s)
print("patched needle")
