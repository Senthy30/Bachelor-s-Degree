file = open("teams.txt", "w")

def GetNextName(currName):
    newName = ""
    carryVal = 1
    i = len(currName) - 1
    while i >= 0:
        ch = ord(currName[i]) + carryVal
        carryVal = 0
        i -= 1
        if ch > ord('z'):
            ch = ch - ord('z') + ord('a') - 1
            carryVal += 1
        newName = chr(ch) + newName
    if carryVal > 0:
        newName = 'a' + newName

    return newName


maxCountTeams = 500
currCountTeam = 0
currTeamName = 'a'
while currCountTeam <= maxCountTeams:
    file.write(currTeamName.upper() + " = " + str(currCountTeam) + ",\n")
    currCountTeam += 1
    currTeamName = GetNextName(currTeamName)