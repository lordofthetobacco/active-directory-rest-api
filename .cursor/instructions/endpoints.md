# Users

GET /user
POST /user
GET /user/:user
PUT /user/:user
GET /user/:user/exists
GET /user/:user/member-of/:group
POST /user/:user/authenticate
PUT /user/:user/password
PUT /user/:user/password-never-expires
PUT /user/:user/password-expires
PUT /user/:user/enable
PUT /user/:user/disable
PUT /user/:user/move
PUT /user/:user/unlock
DELETE /user/:user

# Groups

GET /group
POST /group
GET /group/:group
GET /group/:group/exists
POST /group/:group/user/:user
DELETE /group/:group/user/:user
DELETE /group/:group

# Organizational Units

GET /ou
POST /ou
GET /ou/:ou
GET /ou/:ou/exists
DELETE /ou/:ou

# Other

GET /other
GET /all
GET /find/:filter
GET /status

# Monitoring

GET /status
