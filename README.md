link video demo: https://youtu.be/zJa7xOMSXmA

# Cách sử dụng Terminal Service
  ## Xem Terminal Service:
  Ấn "Window" --> Gõ tìm kiếm "Services" --> Tìm Service: **NewWindowsService**

  ## Cách chạy Terminal Service:
  1. Build chương trình
  2. Mở Termial(Admin)
  3. Tạo Service: 
  ```cmd
  sc.exe create NewWindowsService binPath= "path\to\your\NewWindowsService\NewWindowsService\bin\Debug\NewWindowsService.exe"
  ```
  4. Chạy Service:
  ```cmd
  net start NewWindowsService
  ```
  
  ## Cách dừng/tắt Terminal Service:
  1. Mở Terminal(Admin)
  2. Dừng Service:
  ```cmd
  net stop NewWindowsService
  ```
  3. Xóa Service:
  ```cmd
  sc.exe delete NewWindowsService
  ```
  
  ## File Log:
  - Vị trí: C:/Logs/
  - File Log sẽ rotate 10 file, mỗi file 50 MB

# Cách sử dụng Project
  ## I. Cài đặt TerminalService:
  Mở NewWindowsService\TerminalService-Setup\Debug. Nhấp vào **setup.exe**.
  ![image](https://github.com/user-attachments/assets/de210a4a-c9af-4fd5-b535-2ad3c6654c92)

  ## II. Chạy Terminal Service (được chạy trên các máy con trong phòng máy):

  - Cách chạy: (Hướng dẫn ở trên)
  - Xem Terminal Service (Hướng dẫn ở trên)
  ![image](https://github.com/user-attachments/assets/e667bfbe-b00b-475b-93c0-26e399a3a5ab)

  - Chuột phải vào NewWindowsService, chọn *Properties*, chọn *Log On*
  
  ![image](https://github.com/user-attachments/assets/1e8810b9-5b70-4934-be47-140e21de6626)

  - Chọn *This account*, điền tên và mật khẩu người dùng hiện tại hoặc người dùng khác có trên máy. Sau đó nhấn *Apply*.
  - **Lưu ý:** tài khoản này phải là tài khoản Local (không phải tài khoản Microsoft) và phải được cho quyền quản trị để dễ dàng điều chỉnh.

  ## III. Chạy TerminalConf để sửa file cấu hình của Terminal Service:
  - Cài đặt ở TerminalConf\TerminalConf-Setup\Debug. Nhấp vào setup.exe
  - Mở Terminal. Chuyển đến nơi chứa file TerminalConf.exe.
    ```cmd
    cd C:\Program Files (x86)\hunghyperx@gmail.com\TerminalConf-SetUp
    ```
  - Gõ lệnh trên để sửa số phòng (ví dụ B1-206), số máy (22), COM PORT của thiết bị nhúng kết nối vào máy (COM3)  hoặc/và MQTT broker 
    ```cmd
    TerminalConf.exe -r B1-206 -i 22 -l COM3
    ```
  - Có thể gõ TerminalConf.exe để thấy hướng dẫn.
  - Muốn Terminal Service ghi nhận sự thay đổi thì phải Restart lại NewWindowsService.

  ## IV. Thiết bị nhúng
  ### 1. Cách chạy:
  - Nạp code TestBoard.ino vào thiết bị.
    ![9793fb66-fcec-43c6-b3cc-3fdcddd6eab7](https://github.com/user-attachments/assets/f2c17277-f469-4076-893b-7270438d8db7)

  - Giữ nút BOOT khi nạp code. Khi thấy hiện ra chứ Connecting.... thì hãy nhấn nút RST rồi thả ra, sau 1 giây thả nút BOOT ra. Rồi khi code nạp xong hãy nhất RST 1 lần. 
  - Thiết lập mặc định *ComPort* là COM7, và thiệt lập *Serial* là 115200.
  - Lưu ý: Khi đang chạy Service thì tắt ArduinoIDE và các chương trình có liên quan đến kết nối với cổng COM của thiết bị nhúng. Vì vậy trước khi nạp code, hãy dừng Terminal Service lại, rồi sau đó hãy nạp code, rồi tắt ArduinoIDE rồi mới bật lại Terminal Service lên để tránh xung đột dẫn đến không chạy được.

  ### 2. Chức năng:
  ![496622981_1047667870625057_7645876497791463438_n](https://github.com/user-attachments/assets/4626955d-183e-4614-9bee-8712fc690eb4)
  - Nhấn SW1 hoặc SW2 để đổi thông tin muốn xem của máy
  - Nhấn SW3 để đổi màu đèn
  - Nhấn SW4 để reset mã thẻ sinh viên
  - Quét thẻ sinh viên qua cuộn đồng để đọc thẻ sinh viên

  ## V. Chạy LocalCenterForm (chạy trên máy trạm để theo dõi các máy con)
  ### 1. Cách cài đặt:
  - Mở NewWindowsService\LocalCenterForm-Setup\Debug. Nhấp vào setup.exe để cài đặt.
  ### 2. Cách chạy:
  - Chạy C:\Program Files (x86)\hunghyperx\LocalCenterForm-Setup\LocalCenterForm.exe
  ![image](https://github.com/user-attachments/assets/ba550a6b-9b68-4a94-825d-d7eace653b96)

  


  
     
