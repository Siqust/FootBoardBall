mx=13
mn=3
attack=[]
import colorsys
for i in range(12):
    attack.append(round(mx - (mx-mn)/12*i))
    print(attack[i])
defence=list(reversed(attack))

colors=[] #40-25 225-210
mx,mn=35,10
for i in range(6):
    val=mn+(mx-mn)/5*i
    x=colorsys.hsv_to_rgb(val/360,1,1)
    print(val)
    colors.append((x[0],x[1],x[2]))
mx,mn=225,200
tmp=[]
for i in range(6):
    val=mx-(mx-mn)/5*i
    x=colorsys.hsv_to_rgb(val/360,1,1)
    print(val)
    tmp.append((x[0],x[1],x[2]))
colors+=list(reversed(tmp))

gen=True
if gen:
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
                       .replace('{R}',str(colors[i][0])).replace('{G}',str(colors[i][1])).replace('{B}',str(colors[i][2])))
        


