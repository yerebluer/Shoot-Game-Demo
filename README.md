# Shoot-Game-Demo

游戏使用wasd或者上下左右进行移动（unity默认horizontal和vertical轴），鼠标左键射击

一共有5波，第五波无限敌人，每一波的枪械不同，有单发有连发。

## 模块化

1. 对于具有相同属性的内容，先提取重复的部分，抽象为基类，用virtual函数来实现类内的基础功能。如生命体具有当前血量，初始血量，同时能被击打掉血，会死亡等。

2. 对于其中功能属性，提取成接口。比如射击游戏中，所有生命体都具有可被伤害的性质，此时可以添加一个IDamagable接口。

```csharp
IDamagable damagableObject = c.GetComponent<IDamagable>();
if (damagableObject != null)
{
	damagableObject.TakeHit(damage,hitPoint,transform.forward);
}
```

3. 对模块解耦（每一个小模块对应一个文件），模块与模块之间通过接口进行调用
4. 定义枪械、玩家时，通过定义GunController、PlayerController类，暴露gun和player操作的接口，或者使用基类LivingEntity。

## 细节

1. 对于需要在Inspector中调整，并实时查看的物体，通过继承Editor类，重载OnInspectorGUI（）进行操作

```csharp
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator map = target as MapGenerator;

        if (DrawDefaultInspector())
        {
            map.GenerateMap();
        }
        if(GUILayout.Button("Generate Map"))
        {
            map.GenerateMap();
        }
    }
}
```

2. 游戏中敌人是伪随机在没有障碍的空间内生成。在玩家保持不动时，会在脚下生成敌人。



### 不足

目前缺少背包系统

击杀敌人也没有掉落物（只有分数）

敌人只有使用NavMeshAgent进行移动，然后在一定距离内攻击，没有别的行为模式。未来可以对敌人类型进拓展

只考虑一个玩家的情况，没有预留拓展其他玩家的接口