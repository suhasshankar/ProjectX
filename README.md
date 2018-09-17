# ProjectX
Online Test Platform Applications

Please Run atttached scripts to setup db. Added SQL Script File to set up database.

Step-1: Please login to application with admin account and create Guest/student account under Users --> Add Users.
              Admin Credentials:
                                  username : admin
                                  password: admin
Step-2: After creating Guest/Student account and please manually create password by using below script.
                Update Users
                set Password=''
                where UserName like '% username here %'
        Reason: Logic to set/reset password is not implemented Yet.

Step-3: In admin account, assign test to newly added Guest and also with test type in Course --> Assign Test to Students.

step-4: Login to Guest Account with username and password.

Step-5: Take test.
        Things to Note while taking test:
        1. Timer will be running for each questions.
        2. Once timer ends, Selected options is automatically captured if submit in not hit.
        3. To go to next question, Click next question and question skipped is not repeateda again.
Step-6: View Result and download report card.
