[# Cách sử dụng Terminal Service
  ## Xem Terminal Service:
  Ấn "Window" --> Gõ tìm kiếm "Services" --> Tìm Service: NewWindowsService

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

  ## II. Chạy Terminal Service:

  - Cách chạy: (Hướng dẫn ở trên)
  - Xem Terminal Service (Hướng dẫn ở trên)
  ![image](https://github.com/user-attachments/assets/e667bfbe-b00b-475b-93c0-26e399a3a5ab)

  - Chuột phải vào Service, chọn *Properties*, chọn *Log On*
  
  ![image](https://github.com/user-attachments/assets/1e8810b9-5b70-4934-be47-140e21de6626)

  - Chọn *This account*, điền tên và mật khẩu người dùng hiện tại hoặc người dùng khác có trên máy. Sau đó nhấn *Apply*

  ## III. Chạy TerminalConf để sửa file cấu hình của Terminal Service:

  - Mở Terminal. Chuyển đến nơi chứa file TerminalConf.exe.
    ```cmd
    cd <path/to/TerminalConf\TerminalConf\bin\Debug\net8.0>
    ```
  - Gõ lệnh trên để sửa số phòng (ví dụ B1-206) và số máy (22)
    ```cmd
    .\TerminalConf.exe -r B1-206 -i 22
    ```

  
     
