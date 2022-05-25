# SVN对Github管理教程  
### 准备阶段  
* **1.准备一个vpn**
* **2.SVN端口配置，右键后，选择svn，选择setting.在setting页面输入你VPN的端口号**
     .<div align=center><img src="https://github.com/whx-prog/The-Seed-Link-Future/blob/main/Image/SVN%E8%AE%BE%E7%BD%AE%E4%BD%8D%E7%BD%AE.png" width="450" height="350" /></div>.<div align=center><img src="https://github.com/whx-prog/The-Seed-Link-Future/blob/main/Image/SVN%E7%AB%AF%E5%8F%A3%E8%AE%BE%E7%BD%AE.png" width="450" height="300" /></div>      
* **3.在github申请你的授权Key**  
     * **3.1 进入Github个人设置**  
   .<div align=center><img src="https://github.com/whx-prog/The-Seed-Link-Future/blob/main/Image/Github%E8%AE%BE%E7%BD%AE%E4%BD%8D%E7%BD%AE.png" width="450" height="550" /></div>  
     * **3.2 选择Developer Setting**  
   .<div align=center><img src="https://github.com/whx-prog/The-Seed-Link-Future/blob/main/Image/%E5%BC%80%E5%8F%91%E8%80%85%E8%AE%BE%E7%BD%AE.png" width="450" height="550" /></div>  
     * **3.3选择Access Token，并生成Key**  
     .<div align=center><img src="https://github.com/whx-prog/The-Seed-Link-Future/blob/main/Image/Token%E7%94%9F%E6%88%90.png" width="550" height="400" /></div>  
     * **3.4对Key进行设置（仅需要勾选repo就行），以及你的认证密钥授权期限（建议设置成半年）**  
     .<div align=center><img src="https://github.com/whx-prog/The-Seed-Link-Future/blob/main/Image/Token%E5%B1%9E%E6%80%A7.png" width="550" height="400" /></div>  
     * **3.5准备工作完成（注意这个生成的Key要复制保存下来，后面用到）**  
      .<div align=center><img src="https://github.com/whx-prog/The-Seed-Link-Future/blob/main/Image/%E5%AF%86%E9%92%A5%E4%BF%9D%E5%AD%98.png" width="550" height="150" /></div>  
        
          
### 开发阶段  
* **1.到目标仓库去，Fork目标到你自己的仓库下**
* **2.在本地新建文件夹，进入文件夹后右键**  
* **3.选择Checkout，并输入你自己fork过来的Github仓库地址
    **第一次Checkout要求登录（登录使用自己登录github的账户，以及装备阶段生成的密钥Key作为密码输入）  
    .<div align=center><img src="https://github.com/whx-prog/The-Seed-Link-Future/blob/main/Image/%E7%AC%AC%E4%B8%80%E6%AC%A1%E7%99%BB%E5%BD%95.png" width="400" height="400" /></div>   
* **4.非第一次**  
     * **4.1 修改项目后先右键Assets进行Update**  
     * **4.2 Update后进行commit，并写版本更新注释（每次commit之前都必须先update）**  
     * **4.3 若有新增文件可以右键Add添加新增文件，然后commit**  
#### 注意  
     1.每次更新上传只需要上传Assets文件
     2.每次更新必须先Update，再Commit
     3.Update遇到冲突的时候，右键红字进行冲突处理（可上网搜索如何处理SVN冲突）
