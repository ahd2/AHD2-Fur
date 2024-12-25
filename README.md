# AHD2-Fur


## TODO

* 之前做prez测试乱改了一堆东西，后续有待调整。现在这版基本能用。
* 向量插值（顶点传片元的向量要归一化）
* Debugshader应该另外新建出来，不要编译进打包。
* 贴图空间导出成srgb了
* 毛发遮罩贴图和flowmap的颜色空间也没考虑
* Fur贴图初始化用起来很难受，而且初始化的格式也不支持绘制。
* Fur材质
* 使用方法，挂载furobject，renderfeature挂载毛发renderfeature，设置渲染队列在不透明物体之后。然后毛发shader记得挂载noise噪声图和生成毛发遮罩图。毛发材质需要打开GPU instancing
* 

