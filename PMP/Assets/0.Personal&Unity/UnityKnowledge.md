# Batch

合批处理，提高渲染效率的方法。作用于渲染管线的应用阶段，目的在于减少提交数据的次数，将数据一次性提交（给GPU）。

应为减少了提交次数，从而减少DrawCall（Unity中从CPU准备数据Mesh并设置描画状态并调用通知GPU渲染的过程）

## 四种类型

StaticBatch 静态合批

DynamicBatch 动态合批

InstancingBatch

SRP Batch

