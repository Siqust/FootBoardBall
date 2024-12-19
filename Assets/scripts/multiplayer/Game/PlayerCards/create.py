mx=13
mn=3
attack=[]

for i in range(12):
    attack.append(round(mx - (mx-mn)/12*i))
    print(attack[i])
defence=list(reversed(attack))

gen=True
if gen:
    from colour import Color
    colors=list(Color('blue').range_to(Color("orange"),12))
    colors=list(reversed(colors))

    text='''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 4d6c9db0bd77f784bab0249e17aedcd2, type: 3}
  m_Name: {NAME}
  m_EditorClassIdentifier: 
  playerability: 0
  CardImg: {fileID: 0}
  color: {r: {R}, g: {G}, b: {B}, a: 1}
  attack: {ATTACK}
  defence: {DEFENCE}
    '''

    for i in range(len(attack)):
        a,d=str(attack[i]),str(defence[i])
        name=f'P{i+1}'
        with open(name+'.asset','w') as file:
            file.write(text.replace("{ATTACK}",a).replace("{DEFENCE}",d).replace("{NAME}",name)
                       .replace('{R}',str(colors[i].get_red())).replace('{G}',str(colors[i].get_green())).replace('{B}',str(colors[i].get_blue())))
        


