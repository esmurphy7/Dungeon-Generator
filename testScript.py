import sys
import subprocess
import shlex
from optparse import OptionParser

def main():
	parser = OptionParser()
	parser.add_option("--width_range", '--wr',
						action='store',
						dest='width_range',
						help='specifiy width range to test')
	parser.add_option("--height_range", '--hr',
						action='store',
						dest='height_range',
						help='specifiy height range to test')
	parser.add_option("--hseed_range", '--sr',
						action='store',
						dest='seed_range',
						help='specifiy seed range to test')
	global options
	(options, args) = parser.parse_args()
	checkRequiredOptions()

	min_dim = 3
	width_range = int(options.width_range)
	height_range = int(options.height_range)
	seed_range = int(options.seed_range)

	for width in range(min_dim, width_range):
		for height in range(min_dim, height_range):
			for seed in range(0, seed_range):
				filepath = r'tests/%dx%d-%d.txt'%(width, height, seed)
				#print filepath
				file = open(filepath, 'w+')
				cmd = r"DungeonGenerator.exe %d %d %d" % (width, height, seed)
				#print cmd
				cmd.split(' ')
				subprocess.Popen(cmd, stdout=file)
				file.close()
				# print ' %d %d %d \n'%(width,height,seed)

# quit the script if the user didn't specify all the required script options
def checkRequiredOptions():
	reqOpts = getRequiredOpts()
	for option in reqOpts:
		if not option:
			print 'please specify all required script options (type --help or -h for help)'
			sys.exit(2)

# returns a list of all required options
def getRequiredOpts():
	reqOpts = [options.width_range,
				options.height_range,
				options.seed_range]
	return reqOpts

if __name__ == "__main__":
	main()