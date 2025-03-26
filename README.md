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
     
