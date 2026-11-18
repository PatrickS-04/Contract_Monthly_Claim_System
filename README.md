# PROG6212PoE_ST10440987

**Lecturer Feedback from Part 1 PROG**
- Documentation: 18/20
- UML Class Diagram for Databases: 17/20
  - Missing Foreign Keys
- Project Plan: 21/25
  - Missing Dependencies
- GUI/UI: 20/25
  - Programme coordinators and academic managers should be able to view uploaded supporting documents when reviewing claims
  - UI could be improved
- Version Control: 10/10

**Lecturer Feedback from Part 2 PROG**
- Two separate admin views are required: one for the Coordinator to verify or reject claims, and another for the Manager to approve or reject claims
- A claim should first be submitted to the coordinator for verification. Only after it has been verified should the manager be able to approve or reject it
- The coordinator should only be able to verify or reject a claim, not approve it
- Application crashes when trying to submit a claim with a large file (200MB)
- Error handling could be improved. For example, when submitting a claim, ensure that required fields are not left empty or filled with only whitespace, that hours worked are positive, the application does not crash, and that appropriate error messages are displayed to the user

**Part 1 Changes**
I have implemented the feedback that i got back from my lecturer from Part 1.
I have changed the table in the Programme Coordinators And Academic Managers views to allow the download of the supporting documents if any to help in the Approving or Rejection of Lecturers claims.
I have also slightly improved the UI so that it looks somewhat more professional than just plain boring. Ive kept the main color scheme of blue but have added a few hints of lime as well as a web app background image to lift the UI a bit more. I have also changed the colors of some of the buttons to also add a bit of life to the whole web app.

**Part 2 Changes**
I have implemented all those changes to my Part 3 from the feedback given from Part 2.
I have changed the Coordinator and Manager views from being the exact same to allowing Coordinators to only view Pending claims and either Verify or Reject them. I have changed the Manager view to only view Verified and Rejected claims but the Managers can approve only the Verified claims.
I have also fixed the application crash when uploading a big file in the submit claim form. The file has to be 35MB or lower but if anything more and between 500MB, an error message will be displayed.
I have also added error handling both on client-side as well as Server-side.
I have automated the total Claim Amount total calculation.
I have automated the claim Verification and Approval processes by making a claim red if fields dont match the policies and green if it passes all the checks to make it easier for Coordinators to Verify or Reject claims.
I have also created an HR view to Add new users, Update existing user information, as well as a view to see generated reports of Lecturers Approved claims.

**Youtube Video Link:**
https://youtu.be/xHJFZzZHXLQ
